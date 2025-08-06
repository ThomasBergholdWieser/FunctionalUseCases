namespace FunctionalUseCases;

[Serializable]
public class ExecutionException(string message) : Exception(message);