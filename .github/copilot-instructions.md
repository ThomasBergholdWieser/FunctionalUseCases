# FunctionalUseCases .NET Library

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Bootstrap and Build Process
- **Git Setup (CRITICAL)**: Always run `git fetch --unshallow` before building. Shallow clones lack the git history required by Nerdbank.GitVersioning and will cause build failures with "Shallow clone lacks the objects required to calculate version height" errors.
- **Build and test the repository**:
  - `git fetch --unshallow` (CRITICAL: Required for Nerdbank.GitVersioning)
  - `dotnet restore` (40 seconds - dependencies download)
  - `dotnet build --no-restore` (10 seconds after unshallow, 2 seconds if clean)
  - `dotnet test --no-build --verbosity minimal` (3 seconds - 54 tests)
- **Release builds**:
  - `dotnet build --configuration Release` (2-3 seconds)
  - `dotnet test --configuration Release` (5 seconds)
- **Code formatting**:
  - `dotnet format` (9 seconds - fixes formatting issues automatically)
  - `dotnet format --verify-no-changes` (9 seconds - verifies formatting without changes)
- **NuGet packaging**:
  - `dotnet pack FunctionalUseCases/FunctionalUseCases.csproj --configuration Release --output ./artifacts` (3 seconds)

### Timeout Requirements - NEVER CANCEL
- **CRITICAL**: Set timeouts of 120+ seconds for all commands. Build processes can take 40+ seconds, especially on first run.
- git fetch --unshallow: 60 seconds
- dotnet restore: 120 seconds (first time can be slow)
- dotnet build: 60 seconds
- dotnet test: 60 seconds
- dotnet format: 60 seconds

## Validation Scenarios

### ALWAYS Test These Scenarios After Making Changes
1. **Complete build cycle validation**:
   ```bash
   cd /path/to/repo
   git fetch --unshallow
   dotnet clean
   dotnet restore
   dotnet build --no-restore
   dotnet test --no-build --verbosity minimal
   dotnet format --verify-no-changes
   ```

2. **Sample application functionality**:
   ```bash
   cd Sample
   echo "TestUser" | dotnet run
   # Should output greeting with logging information from behaviors
   # Verify both success and failure cases are handled
   ```

3. **Release build validation**:
   ```bash
   dotnet build --configuration Release
   dotnet test --configuration Release
   ```

### Manual Validation Requirements
- **ALWAYS** run the sample application and verify it produces expected output with logging behaviors
- Test both successful use case execution (valid name) and failed execution (empty name)
- Verify use case dispatcher, execution behaviors (logging and timing), and error handling work correctly
- The sample demonstrates the complete functional use case pattern with ExecutionResult error handling

## Project Structure and Key Locations

```
FunctionalUseCases/
├── FunctionalUseCases.sln                    # Main solution file
├── FunctionalUseCases/                       # Core library project
│   ├── FunctionalUseCases.csproj             # Main library project file
│   ├── ExecutionResult.cs                    # Generic and non-generic result types
│   ├── Execution.cs                          # Factory methods for results
│   ├── ExecutionError.cs                     # Rich error information
│   ├── UseCaseDispatcher.cs                  # Mediator with behavior pipeline
│   ├── Interfaces/
│   │   ├── IUseCase.cs                       # Core use case interfaces
│   │   ├── IUseCaseDispatcher.cs             # Dispatcher interface
│   │   └── IExecutionBehavior.cs             # Behavior pipeline interface
│   ├── Extensions/
│   │   ├── ExecutionResultExtensions.cs      # Logging and utility extensions
│   │   └── UseCaseRegistrationExtensions.cs  # DI registration extensions
│   └── Sample/                               # Example implementations
│       ├── SampleUseCase.cs                  # Example use case parameter
│       ├── SampleUseCaseHandler.cs           # Example use case implementation
│       ├── LoggingBehavior.cs                # Example execution behavior
│       └── TimingBehavior.cs                 # Another execution behavior example
├── Sample/                                   # Console demonstration app
│   ├── Sample.csproj                         # Console app project
│   └── Program.cs                            # Demo with execution behaviors
├── FunctionalUseCases.Tests/                # Unit tests (xUnit)
│   ├── FunctionalUseCases.Tests.csproj      # Test project
│   └── *Tests.cs                            # 54+ comprehensive unit tests
├── .github/workflows/
│   ├── pr.yml                               # Pull request CI (build, test)
│   └── nuget-publish.yml                    # NuGet publishing workflow
├── version.json                             # Nerdbank.GitVersioning configuration
└── README.md                               # Comprehensive documentation
```

## Technology Stack and Dependencies

- **.NET 8.0** - Target framework
- **Microsoft.Extensions.DependencyInjection** (8.0.1) - Core DI container
- **Microsoft.Extensions.Logging.Abstractions** (8.0.1) - Logging abstractions for rich error handling
- **Scrutor** (5.0.1) - Automatic service registration scanning
- **Nerdbank.GitVersioning** (3.7.115) - Semantic versioning from Git history
- **xUnit** (2.4.2) - Unit testing framework

## Key Patterns and Concepts

### Use Case Pattern Implementation
- **IUseCaseParameter&lt;TResult&gt;**: Marker interface for use case parameters
- **IUseCase&lt;TParameter, TResult&gt;**: Interface for use case implementations
- **IUseCaseDispatcher**: Mediator that resolves and executes use cases with behavior pipeline
- **ExecutionResult&lt;T&gt;/ExecutionResult**: Functional result types with rich error handling

### Execution Behaviors (Cross-Cutting Concerns)
- **IExecutionBehavior&lt;TParameter, TResult&gt;**: Pipeline behavior interface
- Behaviors wrap use case execution (logging, timing, validation, caching, etc.)
- **IMPORTANT**: Behaviors are NOT automatically registered - must be registered manually:
  ```csharp
  services.AddScoped(typeof(IExecutionBehavior<,>), typeof(LoggingBehavior<,>));
  ```

### Error Handling with ExecutionResult
- Functional approach with implicit conversions
- Rich error information with messages, error codes, log levels, exceptions
- Combine multiple results using `+` operator or `Combine()` method
- Built-in logging integration

## Common Build and Development Tasks

### Adding New Use Cases
1. Create use case parameter implementing `IUseCaseParameter<TResult>`
2. Create use case handler implementing `IUseCase<TParameter, TResult>`
3. Use case registration is automatic via `services.AddUseCasesFromAssemblyContaining<YourUseCase>()`
4. **Always test**: Build, run tests, and run sample to validate

### Adding Execution Behaviors
1. Implement `IExecutionBehavior<TParameter, TResult>`
2. **CRITICAL**: Manual registration required: `services.AddScoped(typeof(IExecutionBehavior<,>), typeof(YourBehavior<,>))`
3. Behaviors execute in registration order

### CI/CD Integration
- **GitHub Actions**: `.github/workflows/pr.yml` runs build and test on PRs
- **NuGet Publishing**: `.github/workflows/nuget-publish.yml` publishes on releases
- **Versioning**: Automatic via Nerdbank.GitVersioning based on Git history

## Known Issues and Workarounds

### Git Shallow Clone Issue
- **Problem**: Build fails with "Shallow clone lacks the objects required to calculate version height"
- **Solution**: Always run `git fetch --unshallow` before building
- **Why**: Nerdbank.GitVersioning requires full Git history to calculate version numbers

### Formatting Issues
- **Problem**: Code may have formatting inconsistencies
- **Solution**: Run `dotnet format` to auto-fix, then `dotnet format --verify-no-changes` to verify
- **Always**: Include formatting verification in your workflow

### Execution Behavior Registration
- **Important**: Execution behaviors are NOT automatically discovered/registered
- **Required**: Manual registration using standard DI: `services.AddScoped(typeof(IExecutionBehavior<,>), typeof(YourBehavior<,>))`
- **Order**: Behaviors execute in the order they are registered in DI container

## Performance Expectations

- **Clean Build**: ~10 seconds (after unshallow)
- **Incremental Build**: ~2 seconds
- **Test Suite**: ~3 seconds (54 tests)
- **Format Check**: ~9 seconds
- **Full CI Cycle**: ~60 seconds total
- **Sample App**: Runs instantly, demonstrates logging and timing behaviors

Always validate that instructions work by running complete scenarios. The sample application is the best validation tool - it exercises the entire use case pattern with behaviors and error handling.