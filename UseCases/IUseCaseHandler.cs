using FunctionalProcessing;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionalUseCases.UseCases
{
    public interface IUseCaseHandler<TUseCase, TResult>
        where TUseCase : IUseCase<TResult>
    {
        Task<ExecutionResult<TResult>> Handle(TUseCase useCase, CancellationToken cancellationToken = default);
    }
}