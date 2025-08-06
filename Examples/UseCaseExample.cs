using FunctionalUseCases.UseCases;
using FunctionalUseCases.UseCases.Sample;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FunctionalUseCases.Examples
{
    /// <summary>
    /// Example demonstrating how to use the UseCase pattern
    /// </summary>
    public class UseCaseExample
    {
        public static async Task RunExample()
        {
            // Setup DI container
            var services = new ServiceCollection();
            services.AddUseCases(Assembly.GetExecutingAssembly());
            var provider = services.BuildServiceProvider();
            
            // Get the dispatcher
            var dispatcher = provider.GetRequiredService<IUseCaseDispatcher>();
            
            // Example 1: Valid use case
            var successUseCase = new SampleUseCase { Name = "World" };
            var successResult = await dispatcher.Dispatch(successUseCase);
            
            Console.WriteLine($"Success case: {successResult.IsSuccess}");
            Console.WriteLine($"Result: {successResult.Value}");
            
            // Example 2: Invalid use case
            var failureUseCase = new SampleUseCase { Name = "" };
            var failureResult = await dispatcher.Dispatch(failureUseCase);
            
            Console.WriteLine($"Failure case: {failureResult.IsSuccess}");
            Console.WriteLine($"Error: {failureResult.ErrorMessage}");
        }
    }
}