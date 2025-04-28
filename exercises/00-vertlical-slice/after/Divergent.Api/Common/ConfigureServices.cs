using Configuration.LiteDb;
using Divergent.Api.Common.Behaviors;
using Divergent.Data;
using Divergent.Data.Migrations;
using FluentValidation;

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

            options.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(ConfigureServices).Assembly, includeInternalTypes: true);

        return services;
    }
}