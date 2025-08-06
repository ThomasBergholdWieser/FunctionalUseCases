# FunctionalUseCases

A minimal mediator-style Use Case Dispatcher library for .NET that implements functional processing of use cases using the Mediator pattern. This library provides a clean way to organize business logic into discrete, testable use cases with dependency injection support.

## Features

- 🎯 **Mediator Pattern**: Clean separation between use cases and their handlers
- 🚀 **Dependency Injection**: Full support for Microsoft.Extensions.DependencyInjection
- 🔍 **Automatic Registration**: Use Scrutor to automatically discover and register handlers
- ✅ **ExecutionResult Pattern**: Functional approach to handling success/failure scenarios
- 🧪 **Testable**: Easy to unit test individual use cases and handlers
- 📦 **Lightweight**: Minimal dependencies and clean API

## Installation

Add the required packages to your project:

```bash
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Scrutor
```

## Quick Start

### 1. Define a Use Case

```csharp
using FunctionalUseCases;

public class GreetUserUseCase : IUseCase<string>
{
    public string Name { get; }

    public GreetUserUseCase(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
```

### 2. Create a Handler

```csharp
using FunctionalUseCases;

public class GreetUserHandler : IUseCaseHandler<GreetUserUseCase, string>
{
    public async Task<ExecutionResult<string>> HandleAsync(GreetUserUseCase useCase, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(useCase.Name))
        {
            return ExecutionResult<string>.Failure("Name cannot be empty");
        }

        var greeting = $"Hello, {useCase.Name}!";
        return ExecutionResult<string>.Success(greeting);
    }
}
```

### 3. Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;
using FunctionalUseCases;

var services = new ServiceCollection();

// Register all use case handlers from the assembly containing GreetUserUseCase
services.AddUseCasesFromAssemblyContaining<GreetUserUseCase>();

var serviceProvider = services.BuildServiceProvider();
```

### 4. Dispatch Use Cases

```csharp
var dispatcher = serviceProvider.GetRequiredService<IUseCaseDispatcher>();

var useCase = new GreetUserUseCase("World");
var result = await dispatcher.DispatchAsync(useCase);

if (result.IsSuccess)
{
    Console.WriteLine(result.Value); // Output: Hello, World!
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

## Core Components

### IUseCase Interface

Marker interface for use cases. All use cases should implement `IUseCase<TResult>`:

```csharp
public interface IUseCase<out TResult> : IUseCase
{
}
```

### IUseCaseHandler Interface

Generic interface for handlers that process use cases:

```csharp
public interface IUseCaseHandler<in TUseCase, TResult>
    where TUseCase : IUseCase<TResult>
{
    Task<ExecutionResult<TResult>> HandleAsync(TUseCase useCase, CancellationToken cancellationToken = default);
}
```

### ExecutionResult<T>

Functional result type that encapsulates success/failure:

```csharp
public class ExecutionResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }

    public static ExecutionResult<T> Success(T value);
    public static ExecutionResult<T> Failure(string errorMessage);
    public static ExecutionResult<T> Failure(string errorMessage, Exception exception);
    public static ExecutionResult<T> Failure(Exception exception);
}
```

### UseCaseDispatcher

Mediator that resolves and executes use case handlers:

```csharp
public interface IUseCaseDispatcher
{
    Task<ExecutionResult<TResult>> DispatchAsync<TResult>(IUseCase<TResult> useCase, CancellationToken cancellationToken = default);
}
```

## Registration Options

The library provides several extension methods for registering use cases:

```csharp
// Register from specific assemblies
services.AddUseCases(new[] { typeof(MyUseCase).Assembly });

// Register from calling assembly
services.AddUseCasesFromAssembly();

// Register from assembly containing a specific type
services.AddUseCasesFromAssemblyContaining<MyUseCase>();

// Specify service lifetime (default is Scoped)
services.AddUseCasesFromAssembly(ServiceLifetime.Transient);
```

## Example Use Cases

The library includes a sample implementation demonstrating the pattern:

- **SampleUseCase**: Takes a name and returns a greeting
- **SampleUseCaseHandler**: Processes the use case with validation

Run the sample application to see it in action:

```bash
cd Sample
dotnet run
```

## Project Structure

```
FunctionalUseCases/
├── FunctionalUseCases.sln                    # Solution file
├── FunctionalUseCases/                       # Main library
│   ├── IUseCase.cs                          # Use case interface
│   ├── IUseCaseHandler.cs                   # Handler interface
│   ├── ExecutionResult.cs                   # Result type
│   ├── UseCaseDispatcher.cs                 # Mediator implementation
│   ├── UseCaseRegistrationExtensions.cs     # DI extensions
│   └── Sample/                              # Sample implementation
│       ├── SampleUseCase.cs                 # Example use case
│       └── SampleUseCaseHandler.cs          # Example handler
├── Sample/                                   # Console application
│   └── Program.cs                           # Demo application
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

## Best Practices

1. **Keep Use Cases Simple**: Each use case should represent a single business operation
2. **Immutable Use Cases**: Make use case properties read-only for thread safety
3. **Validation**: Perform validation in handlers, not in use cases
4. **Error Handling**: Use ExecutionResult to handle both success and failure scenarios
5. **Async Operations**: Always use async/await for potentially long-running operations
6. **Cancellation**: Support cancellation tokens for responsive applications

## Dependencies

- **.NET 8.0** or later
- **Microsoft.Extensions.DependencyInjection** (8.0.1)
- **Scrutor** (5.0.1) - For automatic service registration

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
