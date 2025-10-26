using FlowNetFramework.Persistence.Data.Audits;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FlowNetFramework.Persistence.Data.Interceptors
{
    public class SaveAuditInterceptor : SaveChangesInterceptor
    {
        private readonly IRequestCookieCollection _cookies;

        public SaveAuditInterceptor(IRequestCookieCollection cookies)
        {
            _cookies = cookies;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
                InterceptAudits(eventData);
                return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            InterceptAudits(eventData);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void InterceptAudits(DbContextEventData eventData)
        {
            if (eventData.Context is not null)
            {
                UpdateAuditableEntities(eventData.Context);
            }
        }

        private void UpdateAuditableEntities(DbContext context)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            DateTime utcNow = DateTime.UtcNow.ToUniversalTime();

            var entities = context.ChangeTracker.Entries<IHasFullAudit>().ToList();

            var softDeletedEntities = context.ChangeTracker.Entries<ISoftDeletable>().ToList();

            #region Cookie'den userId alinmasi

            string userId = string.Empty;

            var localeCookie = _cookies;

            if (_cookies != null &&
                _cookies.TryGetValue("Flowa.Current.UserId", out var userIdStr))
            {
                userId = userIdStr;
            }

            #endregion

            if (entities != null && entities.Count > 0)
            {
                foreach (var entity in entities)
                {
                    if (entity.State == EntityState.Added)
                    {
                        SetCurrentDatePropertyValue(entity, nameof(IHasFullAudit.CreatedDate), utcNow);
                        SetCurrentUserPropertyValue(entity, nameof(IHasFullAudit.CreatedBy), userId);
                    }

                    if (entity.State == EntityState.Modified)
                    {
                        SetCurrentDatePropertyValue(entity, nameof(IHasFullAudit.UpdatedDate), utcNow);
                        SetCurrentUserPropertyValue(entity, nameof(IHasFullAudit.UpdatedBy), userId);
                    }
                }
            }

            foreach (var entity in softDeletedEntities)
            {
                if (entity.State == EntityState.Added)
                {
                    SetCurrentSoftDeletePropertyValue(entity, nameof(ISoftDeletable.IsActive), true);
                }
                if (entity.State == EntityState.Deleted)
                {
                    SetCurrentSoftDeletePropertyValue(entity, nameof(ISoftDeletable.IsActive), false);

                    if (entity is IHasFullAudit)
                    {
                        SetCurrentDatePropertyValue(entity, nameof(IHasFullAudit.UpdatedDate), utcNow);
                        SetCurrentUserPropertyValue(entity, nameof(IHasFullAudit.UpdatedBy), userId);
                    }
                }
            }
        }

        static void SetCurrentDatePropertyValue(EntityEntry entry, string propertyName, DateTime utcNow)
        {
            entry.Property(propertyName).CurrentValue = utcNow;
        }

        static void SetCurrentSoftDeletePropertyValue(EntityEntry entry, string propertyName, bool value)
        {
            entry.Property(propertyName).CurrentValue = value;
        }

        static void SetCurrentUserPropertyValue(EntityEntry entry, string propertyName, string userId)
        {
            entry.Property(propertyName).CurrentValue = userId;
        }
    }
}
