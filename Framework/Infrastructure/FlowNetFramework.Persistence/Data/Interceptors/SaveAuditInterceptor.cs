using FlowNetFramework.Persistence.Data.Audits;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlowNetFramework.Persistence.Data.Interceptors
{
    public class SaveAuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SaveAuditInterceptor(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            InterceptAudits(eventData, result, default);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            InterceptAudits(eventData, result, cancellationToken);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void InterceptAudits(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken)
        {
            if (eventData.Context is not null)
            {
                UpdateAuditableEntities(eventData.Context, result, cancellationToken);
            }
        }

        private void UpdateAuditableEntities(
            DbContext context, 
            InterceptionResult<int> result, 
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            DateTime utcNow = DateTime.UtcNow;

            var entities = context.ChangeTracker.Entries<IHasFullAudit>().ToList();

            #region Cookie'den userId alinmasi

            // 1) UserId cookie
            var userId = _httpContextAccessor?.HttpContext?.Request.Cookies.TryGetValue("Flowa.Current.UserId", out var uid) == true
                ? uid
                : "system";

            #endregion

            // 2) Track edilen FullAudit entity'ler
            var entries = context.ChangeTracker.Entries<IHasFullAudit>()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            if (entries.Count == 0)
                return;

            var auditLogs = new List<AuditLog>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    SetCurrentDatePropertyValue(entry, nameof(IHasFullAudit.CreatedDate), utcNow);
                    SetCurrentUserPropertyValue(entry, nameof(IHasFullAudit.CreatedBy), userId);

                    TrySetProperty(entry, "IsActive", true);
                    continue;
                }

                if (entry.State == EntityState.Modified)
                {
                    // 🔴 OLD value
                    var oldSnapshot = entry.OriginalValues.Properties.ToDictionary(
                        p => p.Name,
                        p => entry.OriginalValues[p]);

                    // 🟢 NEW value
                    var newSnapshot = entry.CurrentValues.Properties.ToDictionary(
                        p => p.Name,
                        p => entry.CurrentValues[p]);

                    var oldJson = JsonSerializer.Serialize(oldSnapshot, new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        ReferenceHandler = ReferenceHandler.IgnoreCycles
                    });

                    var newJson = JsonSerializer.Serialize(newSnapshot, new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        ReferenceHandler = ReferenceHandler.IgnoreCycles
                    });

                    // Audit alanları
                    SetCurrentDatePropertyValue(entry, nameof(IHasFullAudit.UpdatedDate), utcNow);
                    SetCurrentUserPropertyValue(entry, nameof(IHasFullAudit.UpdatedBy), userId);

                    // PK
                    var pkValues = entry.Properties
                        .Where(p => p.Metadata.IsPrimaryKey())
                        .Select(p => p.OriginalValue?.ToString() ?? p.CurrentValue?.ToString())
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToArray();

                    auditLogs.Add(new AuditLog
                    {
                        EntityName = entry.Metadata.ClrType.Name,
                        EntityId = pkValues.Length > 0 ? string.Join(",", pkValues) : "",
                        Action = "Update",
                        OldValueJson = oldJson,
                        NewValueJson = newJson,
                        UserId = userId,
                        CreatedBy = userId,
                        CreatedDate = utcNow,
                        IsActive = true,
                    });

                    continue;
                }

                if (entry.State == EntityState.Deleted)
                {
                    // OLD snapshot: OriginalValues'tan al (soft delete uygulamadan önce)
                    var oldSnapshot = entry.OriginalValues.Properties.ToDictionary(
                        p => p.Name,
                        p => entry.OriginalValues[p]);

                    var oldJson = JsonSerializer.Serialize(oldSnapshot, new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        ReferenceHandler = ReferenceHandler.IgnoreCycles
                    });

                    // Soft delete
                    SetCurrentSoftDeletePropertyValue(entry, "IsActive", value: false);

                    // Silmeyi update'e çevir
                    entry.State = EntityState.Modified;

                    // Audit alanları (soft delete = update)
                    SetCurrentDatePropertyValue(entry, nameof(IHasFullAudit.UpdatedDate), utcNow);
                    SetCurrentUserPropertyValue(entry, nameof(IHasFullAudit.UpdatedBy), userId);

                    var pkValues = entry.Properties
                        .Where(p => p.Metadata.IsPrimaryKey())
                        .Select(p => p.OriginalValue?.ToString() ?? p.CurrentValue?.ToString())
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToArray();

                    auditLogs.Add(new AuditLog
                    {
                        EntityName = entry.Metadata.ClrType.Name,
                        EntityId = pkValues.Length > 0 ? string.Join(",", pkValues) : "",
                        Action = "Delete",
                        OldValueJson = oldJson,
                        UserId = userId,
                        CreatedBy = userId,
                        CreatedDate = utcNow,
                        IsActive = true,
                    });
                }
            }

            if (auditLogs.Count > 0)
            {
                context.Set<AuditLog>().AddRange(auditLogs);
            }
        }

        static void SetCurrentDatePropertyValue(
            EntityEntry entry, 
            string propertyName, 
            DateTime utcNow)
        {
            entry.Property(propertyName).CurrentValue = utcNow;
        }

        static void SetCurrentSoftDeletePropertyValue(
            EntityEntry entry, 
            string propertyName, 
            bool value)
        {
            entry.Property(propertyName).CurrentValue = value;
        }

        static void SetCurrentUserPropertyValue(
           EntityEntry entry,
           string propertyName,
           string userId)
        {
            var property = entry.Property(propertyName);
            var propertyType = property.Metadata.ClrType;

            property.CurrentValue = userId;
        }

        static void TrySetProperty(EntityEntry entry, string propertyName, object value)
        {
            var prop = entry.Metadata.FindProperty(propertyName);
            if (prop is null) return;
            entry.Property(propertyName).CurrentValue = value;
        }
    }
}
