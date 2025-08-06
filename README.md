# FunctionalUseCases

A complete .NET solution that implements functional processing of use cases using the Mediator pattern with advanced ExecutionResult error handling. This library provides a clean way to organize business logic into discrete, testable use cases with sophisticated dependency injection support and functional error handling patterns.

## Features

- 🎯 **Mediator Pattern**: Clean separation between use case parameters and their implementations
- 🚀 **Dependency Injection**: Full support for Microsoft.Extensions.DependencyInjection
- 🔍 **Automatic Registration**: Use Scrutor to automatically discover and register use cases
- ✅ **Advanced ExecutionResult Pattern**: Sophisticated functional approach with both generic and non-generic variants
- 🛡️ **Rich Error Handling**: ExecutionError with multiple messages, error codes, and log levels
- 🔄 **Implicit Conversions**: Seamless conversion between values and ExecutionResult
- 🧪 **Testable**: Easy to unit test individual use cases with comprehensive error scenarios
- 📦 **Enterprise-Ready**: Robust implementation with logging integration and cancellation support
- 🔗 **Pipeline Behaviors**: Cross-cutting concerns like logging, validation, caching, and performance monitoring through a clean pipeline pattern

## Installation

Add the required packages to your project:

```bash
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Logging.Abstractions
dotnet add package Scrutor
```

## Quick Start

### 1. Define a Use Case Parameter

```csharp
using FunctionalUseCases;

public class GreetUserUseCase : IUseCaseParameter<string>
{
    public string Name { get; }

    public GreetUserUseCase(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
```

### 2. Create a Use Case Implementation

```csharp
using FunctionalUseCases;

public class GreetUserUseCaseHandler : IUseCase<GreetUserUseCase, string>
{
    public async Task<ExecutionResult<string>> ExecuteAsync(GreetUserUseCase useCaseParameter, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(useCaseParameter.Name))
        {
            return Execution.Failure<string>("Name cannot be empty");
        }

        var greeting = $"Hello, {useCaseParameter.Name}!";
        return Execution.Success(greeting);
    }
}
```

### 3. Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;
using FunctionalUseCases;

var services = new ServiceCollection();

// Register all use cases from the assembly containing GreetUserUseCase
services.AddUseCasesFromAssemblyContaining<GreetUserUseCase>();

var serviceProvider = services.BuildServiceProvider();
```

### 4. Execute Use Cases

```csharp
var dispatcher = serviceProvider.GetRequiredService<IUseCaseDispatcher>();

var useCaseParameter = new GreetUserUseCase("World");
var result = await dispatcher.ExecuteAsync(useCaseParameter);

if (result.ExecutionSucceeded)
{
    Console.WriteLine(result.CheckedValue); // Output: Hello, World!
}
else
{
    Console.WriteLine($"Error: {result.Error?.Message}");
}
```

## Core Components

### IUseCaseParameter Interface

Marker interface for use case parameters. All use case parameters should implement `IUseCaseParameter<TResult>`:

```csharp
public interface IUseCaseParameter<out TResult> : IUseCaseParameter
{
}
```

*Located in: `FunctionalUseCases/Interfaces/IUseCase.cs`*

### IUseCase Interface

Generic interface for use case implementations that process use case parameters:

```csharp
public interface IUseCase<in TUseCaseParameter, TResult>
    where TUseCaseParameter : IUseCaseParameter<TResult>
    where TResult : notnull
{
    Task<ExecutionResult<TResult>> ExecuteAsync(TUseCaseParameter useCaseParameter, CancellationToken cancellationToken = default);
}
```

*Located in: `FunctionalUseCases/Interfaces/IUseCaseHandler.cs`*

### ExecutionResult<T> and ExecutionResult

Advanced functional result types that encapsulate success/failure with rich error information:

```csharp
// Generic variant
public record ExecutionResult<T>(ExecutionError? Error = null) : ExecutionResult(Error) where T : notnull
{
    public bool ExecutionSucceeded { get; }
    public bool ExecutionFailed { get; }
    public T CheckedValue { get; } // Throws if failed
}

// Non-generic variant
public record ExecutionResult(ExecutionError? Error = null)
{
    public bool ExecutionSucceeded { get; }
    public bool ExecutionFailed { get; }
    public ExecutionError CheckedError { get; }
}

// Factory methods via Execution class
var success = Execution.Success("Hello World");
var failure = Execution.Failure<string>("Something went wrong");
var failureWithException = Execution.Failure<string>("Error message", exception);

// Implicit conversion
ExecutionResult<string> result = "Hello World"; // Automatically creates success result
```

### ExecutionError

Rich error information with support for multiple messages, error codes, and logging levels:

```csharp
public record ExecutionError(
    string Message,
    string? ErrorCode = null,
    LogLevel LogLevel = LogLevel.Error,
    Exception? Exception = null,
    IDictionary<string, object>? Properties = null
);
```

### IUseCaseDispatcher

Mediator that resolves and executes use cases:

```csharp
public interface IUseCaseDispatcher
{
    Task<ExecutionResult<TResult>> ExecuteAsync<TResult>(IUseCaseParameter<TResult> useCaseParameter, CancellationToken cancellationToken = default)
        where TResult : notnull;
}
```

*Located in: `FunctionalUseCases/Interfaces/IUseCaseDispatcher.cs`*

## Pipeline Behaviors

Pipeline behaviors allow you to implement cross-cutting concerns like logging, validation, caching, performance monitoring, and more. They wrap around use case execution in a clean, composable way.

### IPipelineBehavior Interface

```csharp
public interface IPipelineBehavior<in TUseCaseParameter, TResult>
    where TUseCaseParameter : IUseCaseParameter<TResult>
    where TResult : notnull
{
    Task<ExecutionResult<TResult>> HandleAsync(TUseCaseParameter useCaseParameter, PipelineBehaviorDelegate<TResult> next, CancellationToken cancellationToken = default);
}
```

*Located in: `FunctionalUseCases/Interfaces/IPipelineBehavior.cs`*

### Creating a Pipeline Behavior

```csharp
using Microsoft.Extensions.Logging;

public class LoggingBehavior<TUseCaseParameter, TResult> : IPipelineBehavior<TUseCaseParameter, TResult>
    where TUseCaseParameter : IUseCaseParameter<TResult>
    where TResult : notnull
{
    private readonly ILogger<LoggingBehavior<TUseCaseParameter, TResult>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TUseCaseParameter, TResult>> logger)
    {
        _logger = logger;
    }

    public async Task<ExecutionResult<TResult>> HandleAsync(TUseCaseParameter useCaseParameter, PipelineBehaviorDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        var useCaseParameterName = typeof(TUseCaseParameter).Name;
        
        _logger.LogInformation("Starting execution of use case: {UseCaseParameterName}", useCaseParameterName);
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var result = await next().ConfigureAwait(false);
            
            stopwatch.Stop();
            
            if (result.ExecutionSucceeded)
            {
                _logger.LogInformation("Successfully executed use case: {UseCaseParameterName} in {ElapsedMilliseconds}ms", 
                    useCaseParameterName, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning("Use case execution failed: {UseCaseParameterName} in {ElapsedMilliseconds}ms. Error: {ErrorMessage}", 
                    useCaseParameterName, stopwatch.ElapsedMilliseconds, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Exception occurred during use case execution: {UseCaseParameterName} in {ElapsedMilliseconds}ms", 
                useCaseParameterName, stopwatch.ElapsedMilliseconds);
            
            return Execution.Failure<TResult>($"Exception in LoggingBehavior: {ex.Message}", ex);
        }
    }
}
```

### Automatic Registration

Pipeline behaviors are automatically registered when you call the registration extension methods:

```csharp
// Register use cases and behaviors from assembly
services.AddUseCasesFromAssemblyContaining<GreetUserUseCase>();

// The LoggingBehavior<TUseCaseParameter, TResult> will be automatically registered
// and will execute for all use cases
```

### Execution Order

Behaviors are executed in the order they are registered. Each behavior can execute logic before and after the next step in the pipeline:

```
Behavior 1 (before) → Behavior 2 (before) → Use Case Handler → Behavior 2 (after) → Behavior 1 (after)
```

### Common Pipeline Behavior Patterns

**Validation Behavior:**
```csharp
public class ValidationBehavior<TUseCaseParameter, TResult> : IPipelineBehavior<TUseCaseParameter, TResult>
    where TUseCaseParameter : IUseCaseParameter<TResult>
    where TResult : notnull
{
    public async Task<ExecutionResult<TResult>> HandleAsync(TUseCaseParameter useCaseParameter, PipelineBehaviorDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        // Perform validation logic
        if (/* validation fails */)
        {
            return Execution.Failure<TResult>("Validation failed");
        }
        
        return await next().ConfigureAwait(false);
    }
}
```

**Caching Behavior:**
```csharp
public class CachingBehavior<TUseCaseParameter, TResult> : IPipelineBehavior<TUseCaseParameter, TResult>
    where TUseCaseParameter : IUseCaseParameter<TResult>
    where TResult : notnull
{
    private readonly IMemoryCache _cache;

    public async Task<ExecutionResult<TResult>> HandleAsync(TUseCaseParameter useCaseParameter, PipelineBehaviorDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{typeof(TUseCaseParameter).Name}_{useCaseParameter.GetHashCode()}";
        
        if (_cache.TryGetValue(cacheKey, out ExecutionResult<TResult> cachedResult))
        {
            return cachedResult;
        }
        
        var result = await next().ConfigureAwait(false);
        
        if (result.ExecutionSucceeded)
        {
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        }
        
        return result;
    }
}
```

## Registration Options

The library provides several extension methods for registering use cases (*located in: `FunctionalUseCases/Extensions/UseCaseRegistrationExtensions.cs`*):

```csharp
// Register from specific assemblies
services.AddUseCases(new[] { typeof(MyUseCaseParameter).Assembly });

// Register from calling assembly
services.AddUseCasesFromAssembly();

// Register from assembly containing a specific type
services.AddUseCasesFromAssemblyContaining<MyUseCaseParameter>();

// Specify service lifetime (default is Transient)
services.AddUseCasesFromAssembly(ServiceLifetime.Scoped);
```

## Advanced ExecutionResult Features

### Implicit Conversions
```csharp
// Implicit conversion from value to success result
ExecutionResult<string> result = "Hello World";

// Explicit failure creation
var failure = Execution.Failure<string>("Something went wrong");
```

### Error Handling Patterns
```csharp
var result = await dispatcher.ExecuteAsync(useCaseParameter);

// Pattern 1: Check success and access value
if (result.ExecutionSucceeded)
{
    var value = result.CheckedValue; // Safe access to value
    Console.WriteLine(value);
}

// Pattern 2: Handle failure
if (result.ExecutionFailed)
{
    var error = result.Error;
    Console.WriteLine($"Error: {error?.Message}");
    
    // Access additional error information
    Console.WriteLine($"Error Code: {error?.ErrorCode}");
    Console.WriteLine($"Log Level: {error?.LogLevel}");
    
    if (error?.Exception != null)
    {
        Console.WriteLine($"Exception: {error.Exception.Message}");
    }
}

// Pattern 3: Throw on failure
result.ThrowIfFailed("Custom error message");
```

### Logging Integration
```csharp
// ExecutionResult integrates with Microsoft.Extensions.Logging
var result = Execution.Failure<string>("Database connection failed", 
    errorCode: "DB_001", 
    logLevel: LogLevel.Critical);

// Use logging extensions
result.LogIfFailed(logger, "Failed to process user request");
```

## Example Use Cases

The library includes a comprehensive sample implementation demonstrating the pattern:

- **SampleUseCase**: Use case parameter containing a name for greeting generation
- **SampleUseCaseHandler**: Use case implementation that processes the parameter with validation and business logic using ExecutionResult API

Run the sample application to see it in action:

```bash
cd Sample
dotnet run
```

### Sample Implementation

**Use Case Parameter:**
```csharp
public class SampleUseCase : IUseCaseParameter<string>
{
    public string Name { get; }

    public SampleUseCase(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
```

**Use Case Implementation:**
```csharp
public class SampleUseCaseHandler : IUseCase<SampleUseCase, string>
{
    public async Task<ExecutionResult<string>> ExecuteAsync(SampleUseCase useCaseParameter, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(useCaseParameter.Name))
        {
            return Execution.Failure<string>("Name cannot be empty or whitespace");
        }

        var greeting = $"Hello, {useCaseParameter.Name}! Welcome to FunctionalUseCases.";
        return Execution.Success(greeting);
    }
}
```

**Usage:**
```csharp
var dispatcher = serviceProvider.GetRequiredService<IUseCaseDispatcher>();
var useCaseParameter = new SampleUseCase("World");
var result = await dispatcher.ExecuteAsync(useCaseParameter);

if (result.ExecutionSucceeded)
    Console.WriteLine(result.CheckedValue); // "Hello, World! Welcome to FunctionalUseCases."
else
    Console.WriteLine(result.Error?.Message);
```

## Project Structure

```
FunctionalUseCases/
├── FunctionalUseCases.sln                    # Solution file
├── FunctionalUseCases/                       # Main library
│   ├── ExecutionResult.cs                   # Result types (generic & non-generic)
│   ├── Execution.cs                         # Factory methods
│   ├── ExecutionError.cs                    # Error types
│   ├── ExecutionException.cs                # Exception type
│   ├── UseCaseDispatcher.cs                 # Mediator implementation with pipeline support
│   ├── PipelineBehaviorDelegate.cs           # Pipeline behavior delegate type
│   ├── Interfaces/                          # All interfaces
│   │   ├── IUseCase.cs                      # Use case parameter interfaces
│   │   ├── IUseCaseHandler.cs               # Use case implementation interface
│   │   ├── IUseCaseDispatcher.cs            # Dispatcher interface
│   │   └── IPipelineBehavior.cs             # Pipeline behavior interface
│   ├── Extensions/                          # Extension methods
│   │   ├── ExecutionResultExtensions.cs     # Logging & utility extensions
│   │   └── UseCaseRegistrationExtensions.cs # DI extensions with behavior registration
│   └── Sample/                              # Sample implementation
│       ├── SampleUseCase.cs                 # Example use case parameter
│       ├── SampleUseCaseHandler.cs          # Example use case implementation
│       └── LoggingBehavior.cs               # Example pipeline behavior
├── Sample/                                   # Console application
│   └── Program.cs                           # Demo application with pipeline behaviors
└── README.md                                # This file
```

## Building and Testing

```bash
# Build the solution
dotnet build

# Run the sample
cd Sample && dotnet run

# Run tests (if available)
dotnet test
```

**Sample Output with Pipeline Behaviors:**

```
=== FunctionalUseCases Sample Application with Pipeline Behaviors ===

Example 1: Successful execution
info: Starting execution of use case: SampleUseCase -> String
info: Successfully executed use case: SampleUseCase -> String in 104ms
✅ Success: Hello, World! Welcome to FunctionalUseCases.

Example 2: Failed execution (empty name)
info: Starting execution of use case: SampleUseCase -> String
warn: Use case execution failed: SampleUseCase -> String in 101ms. Error: Name cannot be empty or whitespace
❌ Error: Name cannot be empty or whitespace
```

## Best Practices

1. **Keep Use Case Parameters Simple**: Each use case parameter should represent a single business operation's input data
2. **Immutable Use Case Parameters**: Make use case parameter properties read-only for thread safety
3. **Validation in Use Cases**: Perform validation in use case implementations, not in use case parameters
4. **Rich Error Handling**: Use ExecutionResult with specific error codes and appropriate log levels
5. **Async Operations**: Always use async/await for potentially long-running operations
6. **Cancellation Support**: Support cancellation tokens for responsive applications
7. **Meaningful Names**: Use descriptive names that clearly indicate the business operation being performed
8. **Single Responsibility**: Each use case should handle one specific business scenario
9. **Pipeline Behaviors**: Use behaviors for cross-cutting concerns rather than cluttering use case implementations
10. **Behavior Ordering**: Consider the order of behavior registration as it affects execution order

## Interface Naming

The library uses clear, intent-revealing interface names:
- **IUseCaseParameter**: Represents the data/parameters for a use case
- **IUseCase**: Represents the actual use case implementation/logic
- **IPipelineBehavior**: Represents cross-cutting behavior that wraps use case execution
- **ExecuteAsync**: Method name that clearly indicates execution of business logic
- **HandleAsync**: Method name for pipeline behavior processing

This naming convention follows the principle that parameters define what data is needed, while use cases define how that data is processed, and behaviors define how execution is enhanced.

## Dependencies

- **.NET 8.0** or later
- **Microsoft.Extensions.DependencyInjection** (8.0.1)
- **Microsoft.Extensions.Logging.Abstractions** (8.0.1) - For rich error handling and logging
- **Scrutor** (5.0.1) - For automatic service registration

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
