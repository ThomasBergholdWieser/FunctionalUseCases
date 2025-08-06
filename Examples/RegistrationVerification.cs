using FunctionalUseCases.UseCases;
using FunctionalUseCases.UseCases.Sample;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FunctionalUseCases.Examples
{
    /// <summary>
    /// Verification that the registration system works correctly
    /// </summary>
    public class RegistrationVerification
    {
        public static void VerifyRegistration()
        {
            // Setup DI container
            var services = new ServiceCollection();
            services.AddUseCases(Assembly.GetExecutingAssembly());
            var provider = services.BuildServiceProvider();
            
            // Verify dispatcher is registered
            var dispatcher = provider.GetService<IUseCaseDispatcher>();
            if (dispatcher == null)
                throw new InvalidOperationException("IUseCaseDispatcher not registered");
            
            // Verify sample handler is registered
            var handler = provider.GetService<IUseCaseHandler<SampleUseCase, string>>();
            if (handler == null)
                throw new InvalidOperationException("SampleUseCaseHandler not registered");
            
            // Verify it's the correct type
            if (handler is not SampleUseCaseHandler)
                throw new InvalidOperationException("Wrong handler type registered");
            
            Console.WriteLine("✅ All registrations verified successfully!");
            Console.WriteLine($"✅ Dispatcher type: {dispatcher.GetType().Name}");
            Console.WriteLine($"✅ Handler type: {handler.GetType().Name}");
        }
    }
}