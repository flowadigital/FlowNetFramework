using FlowNetFramework.Persistence.Data.Audits;
using FlowNetFramework.Persistence.Data.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FlowNetFramework.Persistence.Data.EF
{
    public abstract class BaseDbContext : DbContext
    {
        private readonly IRequestCookieCollection _cookies;

        public BaseDbContext(DbContextOptions options, IRequestCookieCollection cookies) : base(options)
        {
            _cookies= cookies;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(new SaveAuditInterceptor(_cookies));
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
