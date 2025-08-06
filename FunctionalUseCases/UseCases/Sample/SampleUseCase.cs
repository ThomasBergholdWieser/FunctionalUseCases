namespace FunctionalUseCases.UseCases.Sample;

/// <summary>
/// Sample use case for demonstration purposes.
/// </summary>
public class SampleUseCase : IUseCase
{
    /// <summary>
    /// Gets or sets the input message for the sample operation.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a flag indicating whether the operation should be uppercase.
    /// </summary>
    public bool ToUpperCase { get; set; }
}