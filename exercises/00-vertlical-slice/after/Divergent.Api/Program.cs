using Configuration.Logging;
using Divergent.Api.Common;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options => options.AddDefaultPolicy(
    policy => policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()));

builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Divergent API", Version = "v1" }));

builder.Services.AddProblemDetails();

builder.Services.AddApplication();

// Add logging my way™
builder.Logging.ClearProviders();
builder.Host.UseSerilog((ctx, services, config) =>
{
    config
        .Enrich.With<LogEventEnricher>()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {lvl}] {ShortSourceContext} - {Message:lj}{NewLine}{Exception:NewLine}",
            theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code
        );
});

builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<Divergent.Data.DivergentDbContext>();
    Divergent.Data.Migrations.DatabaseInitializer.Initialize(dbContext);
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseCors();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error-development");
}
else
{
    app.UseExceptionHandler("/error");
}

app.MapControllers();

app.Run();

