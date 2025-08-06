namespace FunctionalProcessing
{
    public class ExecutionResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Value { get; private set; }
        public string? ErrorMessage { get; private set; }

        private ExecutionResult(bool isSuccess, T? value, string? errorMessage)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
        }

        public static ExecutionResult<T> Success(T value)
        {
            return new ExecutionResult<T>(true, value, null);
        }

        public static ExecutionResult<T> Failure(string errorMessage)
        {
            return new ExecutionResult<T>(false, default(T), errorMessage);
        }
    }
}