using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FlowNetFramework.Logging
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFrameworkLogging(this IServiceCollection services, IConfiguration configuration, ConfigureHostBuilder host)
        {
            Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration).WriteTo.Console().Enrich.FromLogContext()
            .CreateLogger();

            host.UseSerilog();

            return services;
        }

    }
}
