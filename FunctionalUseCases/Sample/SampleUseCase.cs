namespace FunctionalUseCases.Sample;

/// <summary>
/// Sample use case parameter that demonstrates the UseCase pattern implementation.
/// This use case parameter contains a name for generating a greeting message.
/// </summary>
public class SampleUseCase : IUseCaseParameter<string>
{
    /// <summary>
    /// Gets the name to greet.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SampleUseCase"/> class.
    /// </summary>
    /// <param name="name">The name to greet.</param>
    public SampleUseCase(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}