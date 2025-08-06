using FunctionalProcessing;
using FunctionalUseCases.UseCases.Core;

namespace FunctionalUseCases.UseCases.Samples;

/// <summary>
/// Handler for the GreetUserUseCase.
/// </summary>
public class GreetUserHandler : IUseCaseHandler<GreetUserUseCase, string>
{
    public Task<ExecutionResult<string>> Handle(GreetUserUseCase useCase, CancellationToken cancellationToken = default)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(useCase.Name))
        {
            var error = new ExecutionError("Name cannot be empty or whitespace");
            return Task.FromResult(new ExecutionResult<string>(error));
        }

        // Process the use case
        var greeting = $"Hello, {useCase.Name}! Welcome to FunctionalUseCases.";
        
        // Return successful result
        return Task.FromResult((ExecutionResult<string>)greeting);
    }
}