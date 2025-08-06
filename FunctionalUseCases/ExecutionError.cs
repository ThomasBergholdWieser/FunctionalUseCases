using Microsoft.Extensions.Logging;

namespace FunctionalUseCases;

public record ExecutionError : ExecutionError<string>
{
    public ExecutionError(IEnumerable<string> messages) : base(messages)
    {
    }

    public ExecutionError(params string[] messages) : base(messages)
    {
    }
}

public record ExecutionError<T>
{
    public ExecutionError(IEnumerable<T> messages)
    {
        this.Messages = messages.ToArray();
    }

    public ExecutionError(params T[] messages)
        : this(messages.AsEnumerable())
    {
    }

    public string Message => string.Join("; ", this.Messages);

    public IList<T> Messages { get; set; } = new List<T>();

    public int? ErrorCode { get; set; }

    public LogLevel LogLevel { get; set; } = LogLevel.Error;

    public bool Logged { get; internal set; } = false;
}