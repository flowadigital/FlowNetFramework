﻿using FlowNetFramework.Persistence.Data.Audits;
using FlowNetFramework.Persistence.Data.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlowNetFramework.Persistence.Data.EF
{
    public abstract class BaseDbContextwithIdentity : IdentityDbContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public BaseDbContextwithIdentity(DbContextOptions options, IHttpContextAccessor httpContextAccessor) : base(options)
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
