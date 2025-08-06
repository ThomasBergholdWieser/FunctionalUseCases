using FunctionalUseCases.UseCases.Core;
using FunctionalUseCases.UseCases.Samples;
using Microsoft.Extensions.DependencyInjection;

// Create a service collection and register use cases
var services = new ServiceCollection();

// Register use cases from the current assembly
services.AddUseCasesFromAssemblyContaining<GreetUserUseCase>();

// Build the service provider
var serviceProvider = services.BuildServiceProvider();

// Get the dispatcher
var dispatcher = serviceProvider.GetRequiredService<IUseCaseDispatcher>();

Console.WriteLine("=== FunctionalUseCases Demo ===\n");

// Example 1: Use case that returns a value (GreetUserUseCase)
Console.WriteLine("1. Greeting User Use Case:");
var greetUseCase = new GreetUserUseCase("Alice");
var greetResult = await dispatcher.Dispatch<GreetUserUseCase, string>(greetUseCase);

if (greetResult.ExecutionSucceeded)
{
    Console.WriteLine($"   Success: {greetResult.CheckedValue}");
}
else
{
    Console.WriteLine($"   Failed: {greetResult.Error}");
}

Console.WriteLine();

// Example 2: Use case that returns a value with validation error
Console.WriteLine("2. Greeting User Use Case (with validation error):");
var greetEmptyUseCase = new GreetUserUseCase("");
var greetEmptyResult = await dispatcher.Dispatch<GreetUserUseCase, string>(greetEmptyUseCase);

if (greetEmptyResult.ExecutionSucceeded)
{
    Console.WriteLine($"   Success: {greetEmptyResult.CheckedValue}");
}
else
{
    Console.WriteLine($"   Failed: {greetEmptyResult.Error}");
}

Console.WriteLine();

// Example 3: Use case that doesn't return a value (LogActionUseCase)
Console.WriteLine("3. Log Action Use Case:");
var logUseCase = new LogActionUseCase("User login successful");
var logResult = await dispatcher.Dispatch(logUseCase);

if (logResult.ExecutionSucceeded)
{
    Console.WriteLine("   Action logged successfully");
}
else
{
    Console.WriteLine($"   Failed to log action: {logResult.Error}");
}

Console.WriteLine();

// Example 4: Use case that doesn't return a value with validation error
Console.WriteLine("4. Log Action Use Case (with validation error):");
var logEmptyUseCase = new LogActionUseCase("");
var logEmptyResult = await dispatcher.Dispatch(logEmptyUseCase);

if (logEmptyResult.ExecutionSucceeded)
{
    Console.WriteLine("   Action logged successfully");
}
else
{
    Console.WriteLine($"   Failed to log action: {logEmptyResult.Error}");
}

Console.WriteLine();

// Example 5: Demonstrate handler not found
Console.WriteLine("5. Handler Not Found Example:");
try
{
    // This would fail because there's no handler for this use case
    var unknownUseCase = new UnknownUseCase();
    var unknownResult = await dispatcher.Dispatch(unknownUseCase);
    
    if (unknownResult.ExecutionSucceeded)
    {
        Console.WriteLine("   Unexpected success");
    }
    else
    {
        Console.WriteLine($"   Expected failure: {unknownResult.Error}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"   Exception: {ex.Message}");
}

Console.WriteLine("\n=== Demo Complete ===");

// Dispose the service provider
await serviceProvider.DisposeAsync();

// Example use case without a handler to demonstrate error handling
public class UnknownUseCase : IUseCase
{
}
