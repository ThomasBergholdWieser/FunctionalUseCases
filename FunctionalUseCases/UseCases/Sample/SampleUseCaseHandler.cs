using FunctionalProcessing;

namespace FunctionalUseCases.UseCases.Sample;

/// <summary>
/// Sample handler that processes SampleUseCase instances.
/// </summary>
public class SampleUseCaseHandler : IUseCaseHandler<SampleUseCase, string>
{
    /// <summary>
    /// Handles the sample use case by processing the message.
    /// </summary>
    /// <param name="useCase">The sample use case to handle.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>An ExecutionResult containing the processed message.</returns>
    public Task<ExecutionResult<string>> HandleAsync(SampleUseCase useCase, CancellationToken cancellationToken = default)
    {
        if (useCase == null)
        {
            return Task.FromResult(new ExecutionResult<string>(new ExecutionError("Use case cannot be null")));
        }

        if (string.IsNullOrWhiteSpace(useCase.Message))
        {
            return Task.FromResult(new ExecutionResult<string>(new ExecutionError("Message cannot be empty")));
        }

        try
        {
            var processedMessage = useCase.ToUpperCase 
                ? useCase.Message.ToUpperInvariant() 
                : useCase.Message.ToLowerInvariant();

            // Use implicit conversion from string to ExecutionResult<string>
            ExecutionResult<string> result = processedMessage;
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ExecutionResult<string>(new ExecutionError($"Error processing message: {ex.Message}")));
        }
    }
}