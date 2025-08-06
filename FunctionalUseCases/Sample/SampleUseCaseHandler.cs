namespace FunctionalUseCases.Sample;

/// <summary>
/// Sample handler that handles the SampleUseCase.
/// Demonstrates how to implement a UseCase handler with business logic.
/// </summary>
public class SampleUseCaseHandler : IUseCaseHandler<SampleUseCase, string>
{
    /// <summary>
    /// Handles the SampleUseCase by creating a greeting message.
    /// </summary>
    /// <param name="useCase">The SampleUseCase to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An ExecutionResult containing the greeting message or error information.</returns>
    public async Task<ExecutionResult<string>> HandleAsync(SampleUseCase useCase, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate some async work
            await Task.Delay(100, cancellationToken);

            // Validate input
            if (string.IsNullOrWhiteSpace(useCase.Name))
            {
                return ExecutionResult<string>.Failure("Name cannot be empty or whitespace");
            }

            // Business logic
            var greeting = $"Hello, {useCase.Name}! Welcome to FunctionalUseCases.";
            
            return ExecutionResult<string>.Success(greeting);
        }
        catch (OperationCanceledException)
        {
            return ExecutionResult<string>.Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            return ExecutionResult<string>.Failure("An error occurred while processing the greeting", ex);
        }
    }
}