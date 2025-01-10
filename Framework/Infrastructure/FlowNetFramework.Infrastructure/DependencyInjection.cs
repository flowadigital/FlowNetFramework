using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FlowNetFramework.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddLoggingServices(this IServiceCollection services, IConfiguration configuration, WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).WriteTo.Console().CreateLogger();

            builder.Host.UseSerilog();

            return services;
        }

    }
}
