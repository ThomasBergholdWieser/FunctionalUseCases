using FunctionalUseCases;
using FunctionalUseCases.Sample;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Create service collection and register UseCase services
var services = new ServiceCollection();

// Add logging services
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

// Register UseCase services using our extension method
services.AddUseCasesFromAssemblyContaining<SampleUseCase>();

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Get the dispatcher
var dispatcher = serviceProvider.GetRequiredService<IUseCaseDispatcher>();

Console.WriteLine("=== FunctionalUseCases Sample Application with Pipeline Behaviors ===\n");

// Example 1: Successful use case execution
Console.WriteLine("Example 1: Successful execution");
var successUseCase = new SampleUseCase("World");
var successResult = await dispatcher.ExecuteAsync(successUseCase);

if (successResult.ExecutionSucceeded)
{
    Console.WriteLine($"✅ Success: {successResult.CheckedValue}");
}
else
{
    Console.WriteLine($"❌ Error: {successResult.Error?.Message}");
}

Console.WriteLine();

// Example 2: Failed use case execution (empty name)
Console.WriteLine("Example 2: Failed execution (empty name)");
var failUseCase = new SampleUseCase("");
var failResult = await dispatcher.ExecuteAsync(failUseCase);

if (failResult.ExecutionSucceeded)
{
    Console.WriteLine($"✅ Success: {failResult.CheckedValue}");
}
else
{
    Console.WriteLine($"❌ Error: {failResult.Error?.Message}");
}

Console.WriteLine();

// Example 3: Interactive example
Console.WriteLine("Example 3: Interactive");
Console.Write("Enter your name: ");
var name = Console.ReadLine();

if (!string.IsNullOrEmpty(name))
{
    var interactiveUseCase = new SampleUseCase(name);
    var interactiveResult = await dispatcher.ExecuteAsync(interactiveUseCase);

    if (interactiveResult.ExecutionSucceeded)
    {
        Console.WriteLine($"✅ {interactiveResult.CheckedValue}");
    }
    else
    {
        Console.WriteLine($"❌ Error: {interactiveResult.Error?.Message}");
    }
}

Console.WriteLine("\nNote: The LoggingBehavior is now automatically registered and will log");
Console.WriteLine("timing and execution details for all use cases to the console.");
Console.WriteLine("\nDone!");
