namespace FunctionalUseCases;

public record ExecutionResult<T>(ExecutionError? Error = null) : ExecutionResult(Error)
	where T : notnull

{
    protected T? Value { get; set; }

    public override bool ExecutionSucceeded => this.Error is null && this.Value is not null;

    public override bool ExecutionFailed => this.Error is not null || this.Value is null;

    public T CheckedValue => this.ExecutionSucceeded ? this.Value! : throw new NullReferenceException();

    public static implicit operator ExecutionResult<T>(T value) => new() { Value = value };

    public override string ToString() =>
        this.ExecutionSucceeded ? nameof(this.ExecutionSucceeded) : nameof(this.ExecutionFailed) + ": " + this.Error;

    public static ExecutionResult operator +(ExecutionResult<T> left, ExecutionResult right) =>
        Execution.Combine(left, right);
}

public record ExecutionResult(ExecutionError? Error = null)
{
	public bool? NoLog { get; internal set; }
	
    public virtual bool ExecutionSucceeded => this.Error is null;

    public virtual bool ExecutionFailed => this.Error is not null;

    public ExecutionError CheckedError => this.Error ?? throw new NullReferenceException();

    public override string ToString() =>
        this.ExecutionSucceeded ? nameof(this.ExecutionSucceeded) : nameof(this.ExecutionFailed) + ": " + this.Error;

    public void ThrowIfFailed(string? exceptionMessage = null)
    {
	    if (!this.ExecutionFailed)
	    {
		    return;
	    }

	    string BuildInternalMessage() =>
		    this.Error is null
			    ? "Unknown Error"
			    : this.Error.Message;

	    throw new ExecutionException(exceptionMessage is null
		    ? BuildInternalMessage()
		    : exceptionMessage + ": " + BuildInternalMessage());
    }

    public static ExecutionResult operator +(ExecutionResult left, ExecutionResult right) =>
        Execution.Combine(left, right);
}