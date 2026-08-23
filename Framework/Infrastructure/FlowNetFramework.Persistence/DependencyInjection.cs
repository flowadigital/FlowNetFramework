using FlowNetFramework.Persistence.Data.Identity.Models;
using FlowNetFramework.Persistence.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowNetFramework.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistenceServices<T>(this IServiceCollection services, IConfiguration configuration, Action<DbContextOptionsBuilder> options) where T : DbContext
        {
            services.AddHttpContextAccessor();
            services.AddScoped<SaveAuditInterceptor>();
            services.AddDbContext<T>((serviceProvider, builder) =>
            {
                options(builder);
                builder.AddInterceptors(serviceProvider.GetRequiredService<SaveAuditInterceptor>());
            });

            services.AddIdentityCore<AppGenericUser>()
                    .AddRoles<AppGenericRole>()
                    .AddEntityFrameworkStores<T>();

            return services;
        }
    }
}
