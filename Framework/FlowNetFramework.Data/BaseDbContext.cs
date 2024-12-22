using FlowNetFramework.Data.Audits;
using FlowNetFramework.Data.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FlowNetFramework.Data
{
    public abstract class BaseDbContext : DbContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public BaseDbContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(new SaveAuditInterceptor(httpContextAccessor));
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType).Property<bool>(nameof(ISoftDeletable.IsActive)).HasDefaultValue(true);
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
