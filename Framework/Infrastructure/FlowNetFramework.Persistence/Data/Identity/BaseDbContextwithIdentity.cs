using FlowNetFramework.Persistence.Data.Audits;
using FlowNetFramework.Persistence.Data.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlowNetFramework.Persistence.Data.Identity
{
    public abstract class BaseDbContextwithIdentity<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> : IdentityDbContext<TUser, TRole, string,
    IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
        where TUser : IdentityUser, new()
        where TRole : IdentityRole<string>, new()
        where TUserClaim : IdentityUserClaim<string>, new()
        where TUserRole : IdentityUserRole<string>, new()
        where TUserLogin : IdentityUserLogin<string>, new()
        where TRoleClaim : IdentityRoleClaim<string>, new()
        where TUserToken : IdentityUserToken<string>, new()
    {
        public BaseDbContextwithIdentity(DbContextOptions options) : base(options)
        {
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
