using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowNetFramework.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistenceServices<T>(this IServiceCollection services, IConfiguration configuration, Action<DbContextOptionsBuilder> options) where T : DbContext
        {
            //services.AddDbContext<T>(
            //    options => options.UseSqlServer(configuration.GetConnectionString("SampleDbContext"))
            //);

            services.AddDbContext<T>(options);
            return services;
        }
    }
}
