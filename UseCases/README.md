# UseCases

This directory contains the UseCase pattern implementation for FunctionalUseCases library.

## Overview

The UseCase pattern provides a clean way to organize business logic into discrete, testable units. Each use case represents a single business operation and is handled by a corresponding handler.

## Core Components

### IUseCase<TResult>
Base interface for all use cases. Use cases are simple data containers that represent a request for a business operation.

```csharp
public class GetUserUseCase : IUseCase<User>
{
    public int UserId { get; set; }
}
```

### IUseCaseHandler<TUseCase, TResult>
Interface for use case handlers. Each handler implements the business logic for a specific use case.

```csharp
public class GetUserUseCaseHandler : IUseCaseHandler<GetUserUseCase, User>
{
    public async Task<ExecutionResult<User>> Handle(GetUserUseCase useCase, CancellationToken cancellationToken = default)
    {
        // Business logic here
        return ExecutionResult<User>.Success(user);
    }
}
```

### IUseCaseDispatcher
Service responsible for dispatching use cases to their appropriate handlers.

```csharp
public interface IUseCaseDispatcher
{
    Task<ExecutionResult<TResult>> Dispatch<TResult>(IUseCase<TResult> useCase, CancellationToken cancellationToken = default);
}
```

## Registration

Use the extension method to register all use case handlers in your DI container:

```csharp
services.AddUseCases(typeof(SampleUseCaseHandler).Assembly);
```

This will automatically:
- Scan the assembly for all classes implementing `IUseCaseHandler<,>`
- Register them with scoped lifetime
- Register the `IUseCaseDispatcher` service

## Usage Example

```csharp
// In your controller or service
public class SampleController : ControllerBase
{
    private readonly IUseCaseDispatcher _dispatcher;
    
    public SampleController(IUseCaseDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
    
    [HttpPost]
    public async Task<IActionResult> SayHello([FromBody] SampleUseCase useCase)
    {
        var result = await _dispatcher.Dispatch(useCase);
        
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        
        return BadRequest(result.ErrorMessage);
    }
}
```

## Sample Implementation

See the `Sample` directory for a complete example:
- `SampleUseCase`: A simple use case that takes a name
- `SampleUseCaseHandler`: Handler that validates and formats a greeting

The sample demonstrates:
- Input validation
- Success and failure scenarios
- Proper error handling using `ExecutionResult<T>`