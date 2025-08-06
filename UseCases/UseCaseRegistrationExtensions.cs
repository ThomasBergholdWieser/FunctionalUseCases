using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace FunctionalUseCases.UseCases
{
    public static class UseCaseRegistrationExtensions
    {
        public static IServiceCollection AddUseCases(this IServiceCollection services, params System.Reflection.Assembly[] assemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IUseCaseHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
            services.AddScoped<IUseCaseDispatcher, UseCaseDispatcher>();
            return services;
        }
    }
}