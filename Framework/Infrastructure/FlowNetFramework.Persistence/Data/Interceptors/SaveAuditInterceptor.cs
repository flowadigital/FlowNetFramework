using FlowNetFramework.Persistence.Data.Audits;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FlowNetFramework.Persistence.Data.Interceptors
{
    internal class SaveAuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SaveAuditInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            try
            {
                InterceptAudits(eventData);
                return base.SavingChanges(eventData, result);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            try
            {
                InterceptAudits(eventData);
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            }
            catch (Exception ex)
            {

                throw;
            }
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

            var userId = "17C42ADD-2E94-488A-8270-2A7D961D2DCC";

            //var localeCookie = _httpContextAccessor?.HttpContext?.Request.Cookies;

            //if (_httpContextAccessor?.HttpContext?.Request?.Cookies != null &&
            //    _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("Flowa.Current.UserId", out var userIdStr))
            //{
            //    userId = userIdStr;
            //}

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
