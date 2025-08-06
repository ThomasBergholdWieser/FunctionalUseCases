using FunctionalUseCases.UseCases.Core;

namespace FunctionalUseCases.UseCases.Samples;

/// <summary>
/// A use case for logging an action that doesn't return a value.
/// </summary>
public class LogActionUseCase : IUseCase
{
    public string Action { get; }
    public DateTime Timestamp { get; }

    public LogActionUseCase(string action)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Timestamp = DateTime.UtcNow;
    }
}