namespace FunctionalUseCases.UseCases.Sample
{
    using System.Threading;
    using System.Threading.Tasks;
    using FunctionalProcessing;

    public class SampleUseCaseHandler : IUseCaseHandler<SampleUseCase, string>
    {
        public Task<ExecutionResult<string>> Handle(SampleUseCase useCase, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(useCase.Name))
            {
                return Task.FromResult(ExecutionResult<string>.Failure("Name is required."));
            }
            return Task.FromResult(ExecutionResult<string>.Success($"Hello, {useCase.Name}!"));
        }
    }
}