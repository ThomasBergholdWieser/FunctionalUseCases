namespace FunctionalUseCases;

/// <summary>
/// Represents the result of executing a use case, containing either a successful result or error information.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public class ExecutionResult<T>
{
    /// <summary>
    /// Gets a value indicating whether the execution was successful.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Gets the result value if the execution was successful.
    /// </summary>
    public T? Value { get; private set; }

    /// <summary>
    /// Gets the error message if the execution failed.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Gets additional error details if available.
    /// </summary>
    public Exception? Exception { get; private set; }

    private ExecutionResult(bool isSuccess, T? value, string? errorMessage, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    /// <summary>
    /// Creates a successful execution result.
    /// </summary>
    /// <param name="value">The result value.</param>
    /// <returns>A successful ExecutionResult.</returns>
    public static ExecutionResult<T> Success(T value)
    {
        return new ExecutionResult<T>(true, value, null);
    }

    /// <summary>
    /// Creates a failed execution result with an error message.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A failed ExecutionResult.</returns>
    public static ExecutionResult<T> Failure(string errorMessage)
    {
        return new ExecutionResult<T>(false, default(T), errorMessage);
    }

    /// <summary>
    /// Creates a failed execution result with an error message and exception.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <returns>A failed ExecutionResult.</returns>
    public static ExecutionResult<T> Failure(string errorMessage, Exception exception)
    {
        return new ExecutionResult<T>(false, default(T), errorMessage, exception);
    }

    /// <summary>
    /// Creates a failed execution result from an exception.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <returns>A failed ExecutionResult.</returns>
    public static ExecutionResult<T> Failure(Exception exception)
    {
        return new ExecutionResult<T>(false, default(T), exception.Message, exception);
    }
}