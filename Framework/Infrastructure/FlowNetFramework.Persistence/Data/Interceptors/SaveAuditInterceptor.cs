using FlowNetFramework.Persistence.Data.Audits;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlowNetFramework.Persistence.Data.Interceptors
{
    public class SaveAuditInterceptor : SaveChangesInterceptor
    {
        /// <summary>
        /// Aynı DbContext SaveChanges turunda ApplyAudits'in bir kez çalışmasını sağlar
        /// (base.SavingChangesAsync → SavingChanges zinciri veya çift AddInterceptors).
        /// </summary>
        private static readonly ConditionalWeakTable<DbContext, AuditPassState> AuditPassStates = new();

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Converters = { new RuntimeObjectConverter() }
        };

        private readonly IHttpContextAccessor _httpContextAccessor;

        public SaveAuditInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ApplyAudits(eventData.Context, isAsync: false, CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            return result;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            await ApplyAudits(eventData.Context, isAsync: true, cancellationToken);

            // base.SavingChangesAsync → SavingChanges zincirine girmiyoruz (çift audit).
            return result;
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            ClearAuditPass(eventData.Context);
            return result;
        }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            ClearAuditPass(eventData.Context);
            return new ValueTask<int>(result);
        }

        public override void SaveChangesFailed(DbContextErrorEventData eventData)
        {
            ClearAuditPass(eventData.Context);
        }

        public override Task SaveChangesFailedAsync(
            DbContextErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            ClearAuditPass(eventData.Context);
            return Task.CompletedTask;
        }

        private async ValueTask ApplyAudits(DbContext? context, bool isAsync, CancellationToken cancellationToken)
        {
            if (context is null || cancellationToken.IsCancellationRequested)
                return;

            var pass = AuditPassStates.GetOrCreateValue(context);
            if (pass.Applied)
                return;

            pass.Applied = true;

            await ApplyAuditsCore(context, isAsync, cancellationToken);
        }

        private static void ClearAuditPass(DbContext? context)
        {
            if (context is null)
                return;

            if (AuditPassStates.TryGetValue(context, out var pass))
                pass.Applied = false;
        }

        private sealed class AuditPassState
        {
            public bool Applied;
        }

        private async ValueTask ApplyAuditsCore(DbContext context, bool isAsync, CancellationToken cancellationToken)
        {
            var utcNow = DateTime.UtcNow;
            var userId = ResolveUserId();

            // SavingChanges interceptor, EF'in TryDetectChanges'inden ÖNCE çalışır.
            // Add/Delete state'i hemen set eder; tracked entity'de property mutate ise
            // state Unchanged kalır ve DetectChanges olmadan Update audit kaçırılır.
            context.ChangeTracker.DetectChanges();

            var entries = context.ChangeTracker.Entries<IHasFullAudit>()
                .Where(e =>
                    e.Entity is not AuditLog &&
                    e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
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
                    TrySetProperty(entry, nameof(ISoftDeletable.IsActive), true);

                    if (ShouldWriteAuditLog(entry))
                    {
                        TryAddAuditLog(
                            auditLogs,
                            context,
                            entry,
                            action: "Create",
                            userId,
                            utcNow,
                            oldJson: null,
                            newJson: Serialize(Snapshot(entry, useOriginal: false)));
                    }

                    continue;
                }

                if (entry.State == EntityState.Deleted)
                {
                    await LoadOriginalValuesFromDatabaseAsync(entry, isAsync, cancellationToken);

                    var oldJson = Serialize(Snapshot(entry, useOriginal: true));

                    TrySetProperty(entry, nameof(ISoftDeletable.IsActive), false);
                    entry.State = EntityState.Modified;
                    PreserveCreateAuditFields(entry);
                    SetCurrentDatePropertyValue(entry, nameof(IHasFullAudit.UpdatedDate), utcNow);
                    SetCurrentUserPropertyValue(entry, nameof(IHasFullAudit.UpdatedBy), userId);

                    if (ShouldWriteAuditLog(entry))
                    {
                        TryAddAuditLog(
                            auditLogs,
                            context,
                            entry,
                            action: "Delete",
                            userId,
                            utcNow,
                            oldJson: oldJson,
                            newJson: null);
                    }
                    continue;
                }

                if (entry.State == EntityState.Modified)
                {
                    // Update()/attach sonrası Original==Current olabilir; OldValue için her zaman DB'den çek.
                    await LoadOriginalValuesFromDatabaseAsync(entry, isAsync, cancellationToken);
                    PreserveCreateAuditFields(entry);

                    if (IsSoftDeleteTransition(entry))
                    {
                        var deletedOldJson = Serialize(Snapshot(entry, useOriginal: true));

                        SetCurrentDatePropertyValue(entry, nameof(IHasFullAudit.UpdatedDate), utcNow);
                        SetCurrentUserPropertyValue(entry, nameof(IHasFullAudit.UpdatedBy), userId);

                        if (ShouldWriteAuditLog(entry))
                        {
                            TryAddAuditLog(
                                auditLogs,
                                context,
                                entry,
                                action: "Delete",
                                userId,
                                utcNow,
                                oldJson: deletedOldJson,
                                newJson: null);
                        }
                        continue;
                    }

                    if (!HasBusinessPropertyChange(entry)
                        || HasPendingAudit(context, auditLogs, entry.Metadata.ClrType.Name, GetPrimaryKey(entry), "Create"))
                    {
                        continue;
                    }

                    var oldJson = Serialize(Snapshot(entry, useOriginal: true));

                    SetCurrentDatePropertyValue(entry, nameof(IHasFullAudit.UpdatedDate), utcNow);
                    SetCurrentUserPropertyValue(entry, nameof(IHasFullAudit.UpdatedBy), userId);

                    var newJson = Serialize(Snapshot(entry, useOriginal: false));

                    if (ShouldWriteAuditLog(entry))
                    {
                        TryAddAuditLog(
                            auditLogs,
                            context,
                            entry,
                            action: "Update",
                            userId,
                            utcNow,
                            oldJson: oldJson,
                            newJson: newJson);
                    }
                }
            }

            if (auditLogs.Count > 0)
                context.Set<AuditLog>().AddRange(auditLogs);
        }

        private string ResolveUserId()
        {
            return _httpContextAccessor?.HttpContext?.Request.Cookies.TryGetValue("Flowa.Current.UserId", out var uid) == true
                && !string.IsNullOrWhiteSpace(uid)
                    ? uid
                    : "system";
        }

        /// <summary>
        /// Modified/Deleted için OriginalValues'ı DB satırından yükler.
        /// context.Update() sonrası Original==Current olduğu için şart.
        /// </summary>
        private static async ValueTask LoadOriginalValuesFromDatabaseAsync(
            EntityEntry entry,
            bool isAsync,
            CancellationToken cancellationToken)
        {
            var databaseValues = isAsync
                ? await entry.GetDatabaseValuesAsync(cancellationToken)
                : entry.GetDatabaseValues();

            if (databaseValues is null)
                return;

            entry.OriginalValues.SetValues(databaseValues);
        }

        /// <summary>SoftDelete / IsActive: true → false geçişi.</summary>
        private static bool IsSoftDeleteTransition(EntityEntry entry)
        {
            var isActive = entry.Metadata.FindProperty(nameof(ISoftDeletable.IsActive));
            if (isActive is null)
                return false;

            var prop = entry.Property(nameof(ISoftDeletable.IsActive));
            return prop.OriginalValue is true && prop.CurrentValue is false;
        }

        private static bool ShouldWriteAuditLog(EntityEntry entry)
            => entry.Entity is not INotAudited;

        private static readonly HashSet<string> AuditMetadataProperties = new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(IHasFullAudit.CreatedBy),
            nameof(IHasFullAudit.CreatedDate),
            nameof(IHasFullAudit.UpdatedBy),
            nameof(IHasFullAudit.UpdatedDate),
            nameof(ISoftDeletable.IsActive)
        };

        private static bool HasBusinessPropertyChange(EntityEntry entry)
        {
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey())
                    continue;

                if (AuditMetadataProperties.Contains(property.Metadata.Name))
                    continue;

                if (property.IsModified || !Equals(property.OriginalValue, property.CurrentValue))
                    return true;
            }

            return false;
        }

        private static void PreserveCreateAuditFields(EntityEntry entry)
        {
            RestoreOriginalAndClearModified(entry, nameof(IHasFullAudit.CreatedDate));
            RestoreOriginalAndClearModified(entry, nameof(IHasFullAudit.CreatedBy));
        }

        private static void RestoreOriginalAndClearModified(EntityEntry entry, string propertyName)
        {
            var property = entry.Metadata.FindProperty(propertyName);
            if (property is null)
                return;

            var propertyEntry = entry.Property(propertyName);
            propertyEntry.CurrentValue = propertyEntry.OriginalValue;
            propertyEntry.IsModified = false;
        }

        private static void TryAddAuditLog(
            List<AuditLog> auditLogs,
            DbContext context,
            EntityEntry entry,
            string action,
            string userId,
            DateTime utcNow,
            string? oldJson,
            string? newJson)
        {
            var entityName = entry.Metadata.ClrType.Name;
            var entityId = GetPrimaryKey(entry);

            if (HasPendingAudit(context, auditLogs, entityName, entityId, action))
                return;

            auditLogs.Add(new AuditLog
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                OldValueJson = oldJson,
                NewValueJson = newJson,
                UserId = userId,
                CreatedBy = userId,
                CreatedDate = utcNow,
                IsActive = true,
            });
        }

        private static bool HasPendingAudit(
            DbContext context,
            List<AuditLog> auditLogs,
            string entityName,
            string entityId,
            string action)
        {
            if (auditLogs.Any(a => a.EntityName == entityName && a.EntityId == entityId && a.Action == action))
                return true;

            foreach (var auditEntry in context.ChangeTracker.Entries<AuditLog>())
            {
                if (auditEntry.State != EntityState.Added)
                    continue;

                var audit = auditEntry.Entity;
                if (audit.EntityName == entityName && audit.EntityId == entityId && audit.Action == action)
                    return true;
            }

            return false;
        }

        private static string GetPrimaryKey(EntityEntry entry)
        {
            var pkValues = entry.Properties
                .Where(p => p.Metadata.IsPrimaryKey())
                .Select(p => p.CurrentValue?.ToString() ?? p.OriginalValue?.ToString())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToArray();

            return pkValues.Length > 0 ? string.Join(",", pkValues) : string.Empty;
        }

        private static Dictionary<string, object?> Snapshot(EntityEntry entry, bool useOriginal)
        {
            var snapshot = new Dictionary<string, object?>(entry.Properties.Count());
            foreach (var property in entry.Properties)
            {
                snapshot[property.Metadata.Name] = useOriginal
                    ? property.OriginalValue
                    : property.CurrentValue;
            }

            return snapshot;
        }

        private static string Serialize(Dictionary<string, object?> snapshot)
            => JsonSerializer.Serialize(snapshot, SerializerOptions);

        static void SetCurrentDatePropertyValue(EntityEntry entry, string propertyName, DateTime utcNow)
        {
            var property = entry.Metadata.FindProperty(propertyName);
            if (property is null)
                return;

            entry.Property(propertyName).CurrentValue = utcNow;
        }

        static void SetCurrentUserPropertyValue(EntityEntry entry, string propertyName, string userId)
        {
            var property = entry.Metadata.FindProperty(propertyName);
            if (property is null)
                return;

            entry.Property(propertyName).CurrentValue = userId;
        }

        static void TrySetProperty(EntityEntry entry, string propertyName, object value)
        {
            var property = entry.Metadata.FindProperty(propertyName);
            if (property is null)
                return;

            entry.Property(propertyName).CurrentValue = value;
        }

        private sealed class RuntimeObjectConverter : JsonConverter<object>
        {
            public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => JsonElement.ParseValue(ref reader);

            public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
            {
                if (value is JsonElement element)
                {
                    element.WriteTo(writer);
                    return;
                }

                JsonSerializer.Serialize(writer, value, value.GetType(), options);
            }
        }
    }
}
