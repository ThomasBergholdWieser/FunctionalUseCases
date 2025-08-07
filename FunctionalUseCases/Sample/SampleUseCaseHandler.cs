namespace FunctionalUseCases.Sample;

/// <summary>
/// Sample use case that handles the SampleUseCase parameter.
/// Demonstrates how to implement a UseCase with business logic.
/// </summary>
public class SampleUseCaseHandler : IUseCase<SampleUseCase, string>
{
    /// <summary>
    /// Executes the SampleUseCase by creating a greeting message.
    /// </summary>
    /// <param name="useCaseParameter">The SampleUseCase parameter to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ExecutionResult containing the greeting message or error information.</returns>
    public async Task<ExecutionResult<string>> ExecuteAsync(SampleUseCase useCaseParameter, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate some async work
            await Task.Delay(100, cancellationToken);

            // Validate input
            if (string.IsNullOrWhiteSpace(useCaseParameter.Name))
            {
                return Execution.Failure<string>("Name cannot be empty or whitespace");
            }

            // Business logic
            var greeting = $"Hello, {useCaseParameter.Name}! Welcome to FunctionalUseCases.";

            return Execution.Success(greeting);
        }
        catch (OperationCanceledException)
        {
            return Execution.Failure<string>("Operation was cancelled");
        }
        catch (Exception ex)
        {
            return Execution.Failure<string>("An error occurred while processing the greeting", ex);
        }
    }
}