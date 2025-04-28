using Configuration.LiteDb;
using Divergent.Data;
using Divergent.Data.Migrations;

namespace Divergent.Api.Common;

public static class ConfigureServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(_ =>
        {
            var dbOptions = new LiteDbOptions("divergent", DatabaseInitializer.Initialize);
            return new DivergentDbContext(dbOptions);
        });

        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(ConfigureServices).Assembly);
        });

        return services;
    }
}