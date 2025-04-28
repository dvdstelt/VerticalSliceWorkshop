using System.Diagnostics;
using MediatR;

namespace Divergent.Api.Common.Behaviors;

public class PerformanceBehavior<TRequest, TResponse>(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
{
    readonly Stopwatch stopwatch = new();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        stopwatch.Start();
        var response = await next(cancellationToken);
        stopwatch.Stop();

        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        var requestName = typeof(TRequest).Name;

        if (elapsedMilliseconds > 500)
            logger.LogWarning("Handled {RequestName} in {ElapsedMilliseconds} ms",
                requestName, elapsedMilliseconds);
        else
            logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds} ms",
                requestName, elapsedMilliseconds);

        return response;
    }
}