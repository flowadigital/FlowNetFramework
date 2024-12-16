using FlowNetFramework.Data.Audits;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FlowNetFramework.Data.Interceptors
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
            DateTime utcNow = DateTime.UtcNow;

            var entities = context.ChangeTracker.Entries<IHasFullAudit>().ToList();
            var softDeletedEntities = context.ChangeTracker.Entries<ISoftDeletable>().ToList();

            var userId = string.Empty;
            if (entities != null && entities.Count > 0)
            {
                if (_httpContextAccessor.HttpContext.User.Claims.Where(x => x.Type == "EmployeeId").FirstOrDefault() != null)
                {
                    userId = _httpContextAccessor.HttpContext.User.Claims.Where(x => x.Type == "EmployeeId").FirstOrDefault().Value;
                }
                else
                {
                    throw new ArgumentNullException(userId);
                }

                foreach (var entity in entities)
                {
                    if (entity.State == EntityState.Added)
                    {
                        SetCurrentDatePropertyValue(entity, nameof(IHasFullAudit.CreatedAt), utcNow);
                        SetCurrentUserPropertyValue(entity, nameof(IHasFullAudit.CreatedBy), userId);
                    }

                    if (entity.State == EntityState.Modified)
                    {
                        SetCurrentDatePropertyValue(entity, nameof(IHasFullAudit.ModifiedAt), utcNow);
                        SetCurrentUserPropertyValue(entity, nameof(IHasFullAudit.ModifiedBy), userId);
                    }
                }
            }

            foreach (var entity in softDeletedEntities)
            {
                if (entity.State == EntityState.Added)
                {
                    SetCurrentSoftDeletePropertyValue(entity, nameof(ISoftDeletable.Active), true);
                }
                if (entity.State == EntityState.Deleted)
                {
                    SetCurrentSoftDeletePropertyValue(entity, nameof(ISoftDeletable.Active), false);

                    if (entity is IHasFullAudit)
                    {
                        SetCurrentDatePropertyValue(entity, nameof(IHasFullAudit.ModifiedAt), utcNow);
                        SetCurrentUserPropertyValue(entity, nameof(IHasFullAudit.ModifiedBy), userId);
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
