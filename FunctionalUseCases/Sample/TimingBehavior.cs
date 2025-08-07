using Microsoft.Extensions.Logging;

namespace FunctionalUseCases.Sample;

/// <summary>
/// Sample execution behavior that measures execution time.
/// Demonstrates how multiple behaviors can be chained together.
/// </summary>
/// <typeparam name="TUseCaseParameter">The type of use case parameter being handled.</typeparam>
/// <typeparam name="TResult">The type of result returned by the use case.</typeparam>
public class TimingBehavior<TUseCaseParameter, TResult> : IExecutionBehavior<TUseCaseParameter, TResult>
    where TUseCaseParameter : IUseCaseParameter<TResult>
    where TResult : notnull
{
    private readonly ILogger<TimingBehavior<TUseCaseParameter, TResult>> _logger;

    public TimingBehavior(ILogger<TimingBehavior<TUseCaseParameter, TResult>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExecutionResult<TResult>> ExecuteAsync(TUseCaseParameter useCaseParameter, PipelineBehaviorDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        var useCaseParameterName = typeof(TUseCaseParameter).Name;

        _logger.LogInformation("[TimingBehavior] Before execution of {UseCaseParameterName}", useCaseParameterName);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var result = await next().ConfigureAwait(false);
            stopwatch.Stop();

            _logger.LogInformation("[TimingBehavior] After execution of {UseCaseParameterName} - Total time: {ElapsedMilliseconds}ms",
                useCaseParameterName, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "[TimingBehavior] Exception in {UseCaseParameterName} after {ElapsedMilliseconds}ms",
                useCaseParameterName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}