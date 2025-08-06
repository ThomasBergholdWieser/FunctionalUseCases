namespace FunctionalUseCases.UseCases.Sample
{
    using FunctionalProcessing;

    public class SampleUseCase : IUseCase<string>
    {
        public string? Name { get; set; }
    }
}