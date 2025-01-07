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

            builder.Host.UseSerilog();

            return services;
        }

    }
}
