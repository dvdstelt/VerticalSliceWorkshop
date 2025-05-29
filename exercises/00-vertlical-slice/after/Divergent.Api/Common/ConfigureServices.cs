using Divergent.Api.Common.Behaviors;
using Divergent.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Divergent.Api.Common;

public static class ConfigureServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var dbPath = DivergentDbContext.GetDatabasePath("divergent");
        services.AddDbContext<DivergentDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

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

