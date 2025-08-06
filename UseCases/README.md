# FunctionalUseCases - Minimal Mediator-style Use Case Dispatcher

This directory contains a minimal use case dispatcher (`UseCaseDispatcher`) inspired by the Mediator pattern, using Microsoft.Extensions.DependencyInjection and Scrutor for handler registration and your FunctionalProcessing package for unified result handling.

## Components

- `IUseCase<TResult>`: Marker interface for use cases/requests.
- `IUseCaseHandler<TUseCase, TResult>`: Handler interface for processing use cases.
- `UseCaseDispatcher`: Resolves handlers and dispatches use cases, returning `ExecutionResult<TResult>`.
- `UseCaseRegistrationExtensions`: Extension method for registering all handlers via Scrutor.
- `SampleUseCase` and `SampleUseCaseHandler`: Example use case and handler.

## Registration Example

```csharp
using FunctionalUseCases.UseCases;

// In your Startup.cs or Program.cs (for ASP.NET Core or generic host)
services.AddUseCases(typeof(SampleUseCaseHandler).Assembly);
```

## Usage Example

```csharp
var dispatcher = provider.GetRequiredService<IUseCaseDispatcher>();
var result = await dispatcher.Dispatch(new SampleUseCase { Name = "World" });

if (result.IsSuccess)
    Console.WriteLine(result.Value); // "Hello, World!"
else
    Console.WriteLine(result.ErrorMessage);
```

## Notes

- All handlers must return `ExecutionResult<TResult>` from your FunctionalProcessing package.
- This setup works seamlessly with Microsoft DI and Scrutor for handler discovery and lifetime management.