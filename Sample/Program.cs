using FunctionalUseCases;
using FunctionalUseCases.Sample;
using Microsoft.Extensions.DependencyInjection;

// Create service collection and register UseCase services
var services = new ServiceCollection();

// Register UseCase services using our extension method
services.AddUseCasesFromAssemblyContaining<SampleUseCase>();

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Get the dispatcher
var dispatcher = serviceProvider.GetRequiredService<IUseCaseDispatcher>();

Console.WriteLine("=== FunctionalUseCases Sample Application ===\n");

// Example 1: Successful use case execution
Console.WriteLine("Example 1: Successful execution");
var successUseCase = new SampleUseCase("World");
var successResult = await dispatcher.DispatchAsync(successUseCase);

if (successResult.IsSuccess)
{
    Console.WriteLine($"✅ Success: {successResult.Value}");
}
else
{
    Console.WriteLine($"❌ Error: {successResult.ErrorMessage}");
}

Console.WriteLine();

// Example 2: Failed use case execution (empty name)
Console.WriteLine("Example 2: Failed execution (empty name)");
var failUseCase = new SampleUseCase("");
var failResult = await dispatcher.DispatchAsync(failUseCase);

if (failResult.IsSuccess)
{
    Console.WriteLine($"✅ Success: {failResult.Value}");
}
else
{
    Console.WriteLine($"❌ Error: {failResult.ErrorMessage}");
}

Console.WriteLine();

// Example 3: Interactive example
Console.WriteLine("Example 3: Interactive");
Console.Write("Enter your name: ");
var name = Console.ReadLine();

if (!string.IsNullOrEmpty(name))
{
    var interactiveUseCase = new SampleUseCase(name);
    var interactiveResult = await dispatcher.DispatchAsync(interactiveUseCase);

    if (interactiveResult.IsSuccess)
    {
        Console.WriteLine($"✅ {interactiveResult.Value}");
    }
    else
    {
        Console.WriteLine($"❌ Error: {interactiveResult.ErrorMessage}");
    }
}

Console.WriteLine("\nDone!");
