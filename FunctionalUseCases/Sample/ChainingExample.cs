using FunctionalUseCases;
using FunctionalUseCases.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ChainingExample;

// Sample use case parameters
public record GetUserParameter(int UserId) : IUseCaseParameter<User>;
public record ValidateUserParameter(User User) : IUseCaseParameter<User>;
public record SendWelcomeEmailParameter(User User) : IUseCaseParameter<bool>;

// Sample data model
public record User(int Id, string Name, string Email, bool IsActive);

// Sample use case implementations
public class GetUserUseCase : IUseCase<GetUserParameter, User>
{
    public Task<ExecutionResult<User>> ExecuteAsync(GetUserParameter parameter, CancellationToken cancellationToken = default)
    {
        // Simulate getting user from database
        var user = new User(parameter.UserId, "John Doe", "john@example.com", true);
        return Task.FromResult(Execution.Success(user));
    }
}

public class ValidateUserUseCase : IUseCase<ValidateUserParameter, User>
{
    public Task<ExecutionResult<User>> ExecuteAsync(ValidateUserParameter parameter, CancellationToken cancellationToken = default)
    {
        // Simulate user validation
        if (!parameter.User.IsActive)
        {
            return Task.FromResult(Execution.Failure<User>("User is not active"));
        }

        if (string.IsNullOrEmpty(parameter.User.Email))
        {
            return Task.FromResult(Execution.Failure<User>("User email is required"));
        }

        return Task.FromResult(Execution.Success(parameter.User));
    }
}

public class SendWelcomeEmailUseCase : IUseCase<SendWelcomeEmailParameter, bool>
{
    public Task<ExecutionResult<bool>> ExecuteAsync(SendWelcomeEmailParameter parameter, CancellationToken cancellationToken = default)
    {
        // Simulate sending email
        Console.WriteLine($"Sending welcome email to {parameter.User.Name} at {parameter.User.Email}");
        return Task.FromResult(Execution.Success(true));
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddTransient<IUseCaseDispatcher, UseCaseDispatcher>();
        services.AddTransient<IUseCase<GetUserParameter, User>, GetUserUseCase>();
        services.AddTransient<IUseCase<ValidateUserParameter, User>, ValidateUserUseCase>();
        services.AddTransient<IUseCase<SendWelcomeEmailParameter, bool>, SendWelcomeEmailUseCase>();

        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IUseCaseDispatcher>();

        Console.WriteLine("=== Use Case Chaining Example ===\n");

        // Example 1: Successful chain with result passing
        Console.WriteLine("1. Successful chain execution with result passing:");
        var successResult = await dispatcher
            .StartWith(new GetUserParameter(1))
            .Then(user => new ValidateUserParameter(user))
            .Then(user => new SendWelcomeEmailParameter(user))
            .ExecuteAsync();

        if (successResult.ExecutionSucceeded)
        {
            Console.WriteLine($"   Chain completed successfully. Email sent: {successResult.CheckedValue}");
        }
        else
        {
            Console.WriteLine($"   Chain failed: {successResult.Error?.Message}");
        }

        Console.WriteLine();

        // Example 2: Chain with error and error handling
        Console.WriteLine("2. Chain with error and error handling:");
        var errorResult = await dispatcher
            .StartWith(new GetUserParameter(2))
            .Then(user => new ValidateUserParameter(new User(user.Id, user.Name, "", false))) // Invalid user - no email
            .Then(user => new SendWelcomeEmailParameter(user))
            .OnError(error =>
            {
                Console.WriteLine($"   Error handler called: {error.Message}");
                Console.WriteLine("   Implementing fallback logic...");
                return Task.FromResult(Execution.Success(false)); // Return fallback result
            })
            .ExecuteAsync();

        if (errorResult.ExecutionSucceeded)
        {
            Console.WriteLine($"   Chain completed with error handling. Result: {errorResult.CheckedValue}");
        }
        else
        {
            Console.WriteLine($"   Chain failed even with error handling: {errorResult.Error?.Message}");
        }

        Console.WriteLine();

        // Example 3: Chain without error handling (fails fast)
        Console.WriteLine("3. Chain without error handling (fails fast):");
        var failFastResult = await dispatcher
            .StartWith(new GetUserParameter(3))
            .Then(user => new ValidateUserParameter(new User(user.Id, user.Name, "", false))) // Invalid user - no email
            .Then(user => new SendWelcomeEmailParameter(user))
            .ExecuteAsync();

        if (failFastResult.ExecutionSucceeded)
        {
            Console.WriteLine($"   Chain completed successfully. Result: {failFastResult.CheckedValue}");
        }
        else
        {
            Console.WriteLine($"   Chain failed (fail-fast): {failFastResult.Error?.Message}");
        }

        Console.WriteLine("\n=== Chaining Example Complete ===");
    }
}