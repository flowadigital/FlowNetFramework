using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FlowNetFramework.Logging
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddingFrameworkLogging(this IServiceCollection services, IConfiguration configuration, ConfigureHostBuilder host)
        {
            var tableName = configuration["Logging:PostgreSQL:TableName"];

            Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.PostgreSQL(
                connectionString: configuration.GetConnectionString("DefaultConnection"),
                tableName: "Logs",
                needAutoCreateTable: true)
            .CreateLogger();

            host.UseSerilog();

            return services;
        }

    }
}
