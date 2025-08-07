using FunctionalUseCases;
using FunctionalUseCases.Sample;
using FunctionalUseCases.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Create service collection and register UseCase services
var services = new ServiceCollection();

// Add logging services
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

// Register UseCase services using our extension method
services.AddUseCasesFromAssemblyContaining<SampleUseCase>();

// Register a sample transaction manager for demonstration
services.AddScoped<ITransactionManager, SampleTransactionManager>();

// Register execution behaviors using standard DI registration - they will execute in the order they are resolved by the DI container
services.AddScoped(typeof(IExecutionBehavior<,>), typeof(TimingBehavior<,>));
services.AddScoped(typeof(IExecutionBehavior<,>), typeof(LoggingBehavior<,>));

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Get the dispatcher
var dispatcher = serviceProvider.GetRequiredService<IUseCaseDispatcher>();

Console.WriteLine("=== FunctionalUseCases Sample Application with Execution Behaviors ===\n");

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

// Example 3: Use Case Chaining
Console.WriteLine("Example 3: Use Case Chaining");
var chainResult = await dispatcher
    .StartWith(new SampleUseCase("Chain"))
    .Then(new SampleUseCase("Example"))
    .OnError(error =>
    {
        Console.WriteLine($"Chain error occurred: {error.Message}");
        return Task.FromResult(Execution.Success("Chain error handled"));
    })
    .ExecuteAsync();

if (chainResult.ExecutionSucceeded)
{
    Console.WriteLine($"✅ Chain Success: {chainResult.CheckedValue}");
}
else
{
    Console.WriteLine($"❌ Chain Error: {chainResult.Error?.Message}");
}

Console.WriteLine();

// Example 4: WithBehavior - Per-call transaction behavior (Legacy concrete type API)
Console.WriteLine("Example 4: WithBehavior - Per-call transaction behavior (Legacy concrete type API)");
try
{
    var transactionResult = await dispatcher
        .WithBehavior<TransactionBehavior<SampleUseCase, string>>()
        .ExecuteAsync(new SampleUseCase("Transaction"));
        
    if (transactionResult.ExecutionSucceeded)
    {
        Console.WriteLine($"✅ Transaction Success: {transactionResult.CheckedValue}");
    }
    else
    {
        Console.WriteLine($"❌ Transaction Error: {transactionResult.Error?.Message}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Transaction behavior not available (expected): {ex.Message}");
}

Console.WriteLine();

// Example 4b: WithBehavior - Per-call transaction behavior (New open generic API)
Console.WriteLine("Example 4b: WithBehavior - Per-call transaction behavior (New open generic API)");
try
{
    var transactionResult = await dispatcher
        .WithBehavior(typeof(TransactionBehavior<,>))
        .ExecuteAsync(new SampleUseCase("TransactionGeneric"));
        
    if (transactionResult.ExecutionSucceeded)
    {
        Console.WriteLine($"✅ Transaction Success (Open Generic): {transactionResult.CheckedValue}");
    }
    else
    {
        Console.WriteLine($"❌ Transaction Error (Open Generic): {transactionResult.Error?.Message}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Transaction behavior not available (expected): {ex.Message}");
}

Console.WriteLine();

// Example 5: Use Case Chain with WithBehavior (Legacy concrete type API)
Console.WriteLine("Example 5: Use Case Chain with WithBehavior (Legacy concrete type API)");
try
{
    var chainWithBehaviorResult = await dispatcher
        .StartWith(new SampleUseCase("ChainWith"))
        .WithBehavior<TransactionBehavior<SampleUseCase, string>>()
        .Then(new SampleUseCase("Behavior"))
        .ExecuteAsync();
        
    if (chainWithBehaviorResult.ExecutionSucceeded)
    {
        Console.WriteLine($"✅ Chain with Behavior Success: {chainWithBehaviorResult.CheckedValue}");
    }
    else
    {
        Console.WriteLine($"❌ Chain with Behavior Error: {chainWithBehaviorResult.Error?.Message}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Chain with transaction behavior not available (expected): {ex.Message}");
}

Console.WriteLine();

// Example 5b: Use Case Chain with WithBehavior (New open generic API)
Console.WriteLine("Example 5b: Use Case Chain with WithBehavior (New open generic API)");
try
{
    var chainWithBehaviorResult = await dispatcher
        .StartWith(new SampleUseCase("ChainWithGeneric"))
        .WithBehavior(typeof(TransactionBehavior<,>))
        .Then(new SampleUseCase("BehaviorGeneric"))
        .ExecuteAsync();
        
    if (chainWithBehaviorResult.ExecutionSucceeded)
    {
        Console.WriteLine($"✅ Chain with Behavior Success (Open Generic): {chainWithBehaviorResult.CheckedValue}");
    }
    else
    {
        Console.WriteLine($"❌ Chain with Behavior Error (Open Generic): {chainWithBehaviorResult.Error?.Message}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Chain with transaction behavior not available (expected): {ex.Message}");
}

Console.WriteLine();

// Example 6: Interactive example
Console.WriteLine("Example 6: Interactive");
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

Console.WriteLine("\n=== Testing Result Passing ===");
await TestResultPassing(dispatcher);

Console.WriteLine("\nNote: Execution behaviors (TimingBehavior, LoggingBehavior) are registered using");
Console.WriteLine("services.AddScoped(typeof(IExecutionBehavior<,>), typeof(MyBehavior<,>)) for global behaviors.");
Console.WriteLine("Per-call behaviors can be added using:");
Console.WriteLine("  - .WithBehavior<ConcreteType>() for specific behavior types");
Console.WriteLine("  - .WithBehavior(typeof(OpenGeneric<,>)) for open generic behaviors (NEW!)");
Console.WriteLine("Use case chaining allows sequential execution with fluent syntax and error handling.");
Console.WriteLine("Transaction behaviors can be applied per-call and are chain-aware.");
Console.WriteLine("\nDone!");

static async Task TestResultPassing(IUseCaseDispatcher dispatcher)
{
    // Test the result passing functionality
    var resultPassingChain = await dispatcher
        .StartWith(new SampleUseCase("FirstStep"))
        .Then(result => new SampleUseCase($"SecondStep-{result.Length}"))
        .ExecuteAsync();

    if (resultPassingChain.ExecutionSucceeded)
    {
        Console.WriteLine($"✅ Result passing test success: {resultPassingChain.CheckedValue}");
    }
    else
    {
        Console.WriteLine($"❌ Result passing test failed: {resultPassingChain.Error?.Message}");
    }
}
