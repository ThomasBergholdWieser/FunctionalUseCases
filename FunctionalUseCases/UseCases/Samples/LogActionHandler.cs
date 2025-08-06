using FunctionalProcessing;
using FunctionalUseCases.UseCases.Core;

namespace FunctionalUseCases.UseCases.Samples;

/// <summary>
/// Handler for the LogActionUseCase.
/// </summary>
public class LogActionHandler : IUseCaseHandler<LogActionUseCase>
{
    public Task<ExecutionResult> Handle(LogActionUseCase useCase, CancellationToken cancellationToken = default)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(useCase.Action))
        {
            var error = new ExecutionError("Action cannot be empty or whitespace");
            return Task.FromResult(new ExecutionResult(error));
        }

        // Simulate logging the action
        Console.WriteLine($"[{useCase.Timestamp:yyyy-MM-dd HH:mm:ss} UTC] Action logged: {useCase.Action}");
        
        // Return successful result using the Execution.Success() method
        return Task.FromResult(Execution.Success());
    }
}