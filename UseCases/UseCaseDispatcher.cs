using Microsoft.Extensions.DependencyInjection;
using FunctionalProcessing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionalUseCases.UseCases
{
    public interface IUseCaseDispatcher
    {
        Task<ExecutionResult<TResult>> Dispatch<TResult>(IUseCase<TResult> useCase, CancellationToken cancellationToken = default);
    }

    public class UseCaseDispatcher : IUseCaseDispatcher
    {
        private readonly IServiceProvider _provider;
        public UseCaseDispatcher(IServiceProvider provider) => _provider = provider;

        public async Task<ExecutionResult<TResult>> Dispatch<TResult>(IUseCase<TResult> useCase, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(IUseCaseHandler<,>).MakeGenericType(useCase.GetType(), typeof(TResult));
            dynamic handler = _provider.GetRequiredService(handlerType);
            return await handler.Handle((dynamic)useCase, cancellationToken);
        }
    }
}