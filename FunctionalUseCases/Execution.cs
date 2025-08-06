using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;

namespace FunctionalUseCases;

public static class Execution
{
    private static readonly ExecutionResult VoidSuccess = new();

    public static ExecutionResult<TResult> Success<TResult>(TResult value) where TResult : notnull => value;
        
    public static ExecutionResult Success() =>
        VoidSuccess;

    public static ExecutionResult<TResult> Failure<TResult>(IEnumerable<string> messages, int? errorCode = null, LogLevel logLevel = LogLevel.Error) where TResult : notnull =>
        new(new ExecutionError(messages) { ErrorCode = errorCode, LogLevel = logLevel });
    
    public static ExecutionResult Failure(IEnumerable<string> messages, int? errorCode = null, LogLevel logLevel = LogLevel.Error) =>
        new(new ExecutionError(messages) { ErrorCode = errorCode, LogLevel = logLevel });

    public static ExecutionResult<TResult> Failure<TResult>(Exception exception, LogLevel logLevel = LogLevel.Error, bool suppressPipelineLogging = false) where TResult : notnull =>
        Failure<TResult>(GetExceptionMessages(exception), logLevel: logLevel);

    public static ExecutionResult<TResult> Failure<TResult>(string message, Exception ex, LogLevel logLevel = LogLevel.Error) where TResult : notnull =>
        Failure<TResult>(new[] { message }.Concat(GetExceptionMessages(ex)), logLevel: logLevel);

    public static ExecutionResult<TResult> Failure<TResult>(string message, int? errorCode = null, LogLevel logLevel = LogLevel.Error) where TResult : notnull =>
        Failure<TResult>(new[] { message }, errorCode, logLevel);

    public static ExecutionResult<TResult> Failure<TResult>(ExecutionResult result, int? errorCode = null,
	    LogLevel logLevel = LogLevel.Error) where TResult : notnull => 
		Failure<TResult>(result.CheckedError.Messages, result.CheckedError.Logged, errorCode ?? result.CheckedError.ErrorCode, logLevel);

    public static ExecutionResult Failure(ExecutionResult result, int? errorCode = null, LogLevel logLevel = LogLevel.Error) =>
        Failure(result.CheckedError.Messages, result.CheckedError.Logged, errorCode ?? result.CheckedError.ErrorCode, logLevel);

    public static ExecutionResult Failure(string message, int? errorCode = null, LogLevel logLevel = LogLevel.Error) =>
        Failure(new[] { message }, errorCode, logLevel);

    public static ExecutionResult Failure(string message, Exception ex, int? errorCode = null, LogLevel logLevel = LogLevel.Error) =>
        Failure(new[] { message }.Concat(GetExceptionMessages(ex)), errorCode, logLevel);

    public static ExecutionResult Failure(Exception ex, int? errorCode = null, LogLevel logLevel = LogLevel.Error) =>
        Failure(GetExceptionMessages(ex).ToArray(), errorCode, logLevel);

    public static ExecutionResult Combine<T>(params T[] results)
        where T : ExecutionResult =>
        results.All(x => x.ExecutionSucceeded)
            ? Success()
            : Failure(ConcatMessages(results), ConcatErrorCode(results));

    public static string ToStatusCodeText(HttpStatusCode statusCode) =>
        Regex.Replace(statusCode.ToString(), "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    private static ExecutionResult<T> Failure<T>(IEnumerable<string> messages, bool logged, int? errorCode, LogLevel logLevel) where T : notnull =>
	    new(new ExecutionError(messages) { ErrorCode = errorCode, LogLevel = logLevel, Logged = logged });

	private static ExecutionResult Failure(IEnumerable<string> messages, bool logged, int? errorCode, LogLevel logLevel) =>
        new(new ExecutionError(messages) { ErrorCode = errorCode, LogLevel = logLevel, Logged = logged });
    
    private static int? ConcatErrorCode<T>(params T[] results)
        where T : ExecutionResult =>
        results.Select(x => x.Error?.ErrorCode).FirstOrDefault(x => x is not null);

    private static List<string> ConcatMessages<T>(params T[] results)
        where T : ExecutionResult =>
        results.SelectMany(x => x.Error?.Messages ?? new List<string>()).ToList();

    private static IEnumerable<string> GetExceptionMessages(Exception ex)
    {
        if (ex is AggregateException aggregateException)
        {
            foreach (var innerException in aggregateException.InnerExceptions)
            {
                foreach (var innerMessage in GetExceptionMessages(innerException))
                {
                    yield return innerMessage;
                }
            }
        }
        else
        {
            yield return ex.Message;
        }

        if (ex.InnerException is null)
        {
            yield break;
        }

        foreach (var innerMessage in GetExceptionMessages(ex.InnerException))
        {
            yield return innerMessage;
        }
    }
}