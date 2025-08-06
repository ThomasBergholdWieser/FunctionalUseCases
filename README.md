# FunctionalUseCases
Functional processing of use cases using Mediator pattern

A minimal mediator-style Use Case Dispatcher library that implements the mediator pattern for processing use cases using functional programming principles. This library provides a clean separation between use case definitions and their handlers, with dependency injection support for scalable applications.

## Features

- **Mediator Pattern**: Clean separation between use cases and their handlers
- **Functional Processing**: Uses `ExecutionResult<T>` for functional error handling
- **Dependency Injection**: Full Microsoft DI integration with Scrutor for automatic registration
- **Type Safety**: Strongly typed use cases and handlers with generic constraints
- **Async Support**: Built-in async/await support with cancellation tokens

## Installation

Add the required NuGet packages to your project:

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.8" />
<PackageReference Include="Scrutor" Version="6.1.0" />
<PackageReference Include="FunctionalProcessing" Version="2.0.17-g70c5e73034" />
```

## Quick Start

### 1. Define a Use Case

Create a use case by implementing the `IUseCase` marker interface:

```csharp
using FunctionalUseCases.UseCases;

public class CreateUserUseCase : IUseCase
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
```

### 2. Create a Handler

Implement a handler for your use case:

```csharp
using FunctionalUseCases.UseCases;
using FunctionalProcessing;

public class CreateUserHandler : IUseCaseHandler<CreateUserUseCase, User>
{
    public async Task<ExecutionResult<User>> HandleAsync(
        CreateUserUseCase useCase, 
        CancellationToken cancellationToken = default)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(useCase.Email))
        {
            return new ExecutionResult<User>(new ExecutionError("Email is required"));
        }
        
        // Your business logic here
        var user = new User 
        { 
            Email = useCase.Email, 
            Name = useCase.Name 
        };
        
        // Return success result using implicit conversion
        return user;
    }
}
```

### 3. Register Services

Register the use case dispatcher and handlers in your DI container:

```csharp
using FunctionalUseCases.UseCases;
using Microsoft.Extensions.DependencyInjection;

// Register all use case handlers from the current assembly
services.AddUseCases();

// Or register from a specific assembly
services.AddUseCasesFromAssemblyContaining<CreateUserUseCase>();

// Or register from multiple assemblies
services.AddUseCases(
    typeof(CreateUserUseCase).Assembly,
    typeof(SomeOtherUseCase).Assembly
);
```

### 4. Use the Dispatcher

Inject and use the `UseCaseDispatcher` to execute use cases:

```csharp
public class UserController : ControllerBase
{
    private readonly UseCaseDispatcher _dispatcher;

    public UserController(UseCaseDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var useCase = new CreateUserUseCase 
        { 
            Email = request.Email, 
            Name = request.Name 
        };

        var result = await _dispatcher.DispatchAsync<CreateUserUseCase, User>(
            useCase, 
            HttpContext.RequestAborted
        );

        if (result.ExecutionSucceeded)
        {
            return Ok(result.CheckedValue);
        }
        
        return BadRequest(result.Error.Message);
    }
}
```

## Sample Implementation

The library includes a sample use case and handler for demonstration:

### SampleUseCase

```csharp
public class SampleUseCase : IUseCase
{
    public string Message { get; set; } = string.Empty;
    public bool ToUpperCase { get; set; }
}
```

### SampleUseCaseHandler

```csharp
public class SampleUseCaseHandler : IUseCaseHandler<SampleUseCase, string>
{
    public Task<ExecutionResult<string>> HandleAsync(
        SampleUseCase useCase, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(useCase.Message))
        {
            return Task.FromResult(new ExecutionResult<string>(new ExecutionError("Message cannot be empty")));
        }

        var processedMessage = useCase.ToUpperCase 
            ? useCase.Message.ToUpperInvariant() 
            : useCase.Message.ToLowerInvariant();
            
        // Use implicit conversion for success result
        ExecutionResult<string> result = processedMessage;
        return Task.FromResult(result);
    }
}
```

## Architecture

The library consists of four main components:

1. **IUseCase**: Marker interface for all use cases
2. **IUseCaseHandler<TUseCase, TResult>**: Generic handler interface with functional result handling
3. **UseCaseDispatcher**: Resolves and executes handlers via dependency injection
4. **UseCaseRegistrationExtensions**: Provides convenient registration methods using Scrutor

## Error Handling

The library uses `ExecutionResult<T>` from the FunctionalProcessing library for functional error handling. This allows for:

- **Explicit Error States**: No hidden exceptions, all error states are explicit
- **Functional Composition**: Results can be chained and composed functionally
- **Type Safety**: Compile-time guarantees about success and error handling
- **Implicit Conversion**: Success values can be implicitly converted to ExecutionResult<T>

## Best Practices

1. **Keep Use Cases Simple**: Use cases should only contain data, no business logic
2. **Single Responsibility**: Each handler should handle exactly one use case
3. **Immutable Use Cases**: Consider making use case properties readonly or init-only
4. **Validation**: Perform input validation in handlers, not in use cases
5. **Error Handling**: Always use ExecutionResult<T> for consistent error handling

## License

This project is licensed under the MIT License.