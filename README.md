# FunctionalUseCases

A minimal mediator-like pattern for handling use cases, leveraging the FunctionalProcessing package for result handling.

## Features

- **IUseCase/IUseCaseHandler pattern**: Clean separation between use case definitions and their handlers
- **Functional result handling**: Uses `ExecutionResult` and `ExecutionResult<T>` from FunctionalProcessing package
- **Dependency Injection**: Integrates with Microsoft.Extensions.DependencyInjection
- **Automatic registration**: Uses Scrutor for automatic handler discovery and registration
- **Type-safe dispatching**: UseCaseDispatcher resolves and executes handlers with proper error handling

## Quick Start

### 1. Install Dependencies

The project uses these NuGet packages:
- `FunctionalProcessing` - For functional result handling
- `Microsoft.Extensions.DependencyInjection` - For dependency injection
- `Scrutor` - For automatic handler registration

### 2. Define a Use Case

```csharp
using FunctionalUseCases.UseCases.Core;

// Use case that returns a value
public class GreetUserUseCase : IUseCase<string>
{
    public string Name { get; }

    public GreetUserUseCase(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}

// Use case that doesn't return a value
public class LogActionUseCase : IUseCase
{
    public string Action { get; }
    public DateTime Timestamp { get; }

    public LogActionUseCase(string action)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Timestamp = DateTime.UtcNow;
    }
}
```

### 3. Implement Handlers

```csharp
using FunctionalProcessing;
using FunctionalUseCases.UseCases.Core;

// Handler for use case that returns a value
public class GreetUserHandler : IUseCaseHandler<GreetUserUseCase, string>
{
    public Task<ExecutionResult<string>> Handle(GreetUserUseCase useCase, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(useCase.Name))
        {
            var error = new ExecutionError("Name cannot be empty or whitespace");
            return Task.FromResult(new ExecutionResult<string>(error));
        }

        var greeting = $"Hello, {useCase.Name}! Welcome to FunctionalUseCases.";
        return Task.FromResult((ExecutionResult<string>)greeting);
    }
}

// Handler for use case that doesn't return a value
public class LogActionHandler : IUseCaseHandler<LogActionUseCase>
{
    public Task<ExecutionResult> Handle(LogActionUseCase useCase, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(useCase.Action))
        {
            var error = new ExecutionError("Action cannot be empty or whitespace");
            return Task.FromResult(new ExecutionResult(error));
        }

        Console.WriteLine($"[{useCase.Timestamp:yyyy-MM-dd HH:mm:ss} UTC] Action logged: {useCase.Action}");
        
        return Task.FromResult(Execution.Success());
    }
}
```

### 4. Register Services and Use

```csharp
using FunctionalUseCases.UseCases.Core;
using Microsoft.Extensions.DependencyInjection;

// Create service collection and register use cases
var services = new ServiceCollection();
services.AddUseCasesFromAssemblyContaining<GreetUserUseCase>();

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Get dispatcher
var dispatcher = serviceProvider.GetRequiredService<IUseCaseDispatcher>();

// Execute use cases
var greetUseCase = new GreetUserUseCase("Alice");
var result = await dispatcher.Dispatch<GreetUserUseCase, string>(greetUseCase);

if (result.ExecutionSucceeded)
{
    Console.WriteLine($"Success: {result.CheckedValue}");
}
else
{
    Console.WriteLine($"Failed: {result.Error}");
}

var logUseCase = new LogActionUseCase("User logged in");
var logResult = await dispatcher.Dispatch(logUseCase);

if (logResult.ExecutionSucceeded)
{
    Console.WriteLine("Action logged successfully");
}
else
{
    Console.WriteLine($"Failed: {logResult.Error}");
}
```

## Registration Options

The library provides several registration methods:

```csharp
// Register from assemblies containing specific types
services.AddUseCasesFromAssemblyContaining<MyUseCase>();

// Register from specific assemblies
services.AddUseCases(typeof(MyUseCase).Assembly);

// Register from multiple assemblies
services.AddUseCases(assembly1, assembly2, assembly3);
```

## Architecture

### Core Components

- **IUseCase**: Marker interface for use cases
- **IUseCase<T>**: Marker interface for use cases that return a value
- **IUseCaseHandler<TUseCase>**: Handler interface for use cases without return values
- **IUseCaseHandler<TUseCase, TResult>**: Handler interface for use cases with return values
- **IUseCaseDispatcher**: Interface for dispatching use cases to handlers
- **UseCaseDispatcher**: Default implementation that resolves handlers via DI

### Result Handling

The library uses the FunctionalProcessing package for result handling:

- **ExecutionResult**: Represents the outcome of operations without return values
- **ExecutionResult<T>**: Represents the outcome of operations with return values
- **ExecutionError**: Represents error information
- **Execution.Success()**: Creates successful ExecutionResult instances

### Error Handling

- Missing handlers are automatically detected and result in failed ExecutionResult
- Handler exceptions are caught and wrapped in ExecutionResult
- Validation errors can be returned as ExecutionResult instances

## Sample Project

See the `Sample` project for a complete working example demonstrating:

- Use cases with and without return values
- Successful and failed execution paths
- Error handling for missing handlers
- Input validation patterns

## License

This project is licensed under the MIT License - see the LICENSE file for details.
