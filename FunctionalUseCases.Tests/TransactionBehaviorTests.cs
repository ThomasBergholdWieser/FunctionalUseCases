using Microsoft.Extensions.Logging;

namespace FunctionalUseCases.Tests;

public class TransactionBehaviorTests
{
    private readonly ITransactionManager _mockTransactionManager;
    private readonly ITransaction _mockTransaction;
    private readonly ILogger<TransactionBehavior<TestUseCaseParameter, string>> _mockLogger;
    private readonly TransactionBehavior<TestUseCaseParameter, string> _behavior;

    public TransactionBehaviorTests()
    {
        _mockTransactionManager = A.Fake<ITransactionManager>();
        _mockTransaction = A.Fake<ITransaction>();
        _mockLogger = A.Fake<ILogger<TransactionBehavior<TestUseCaseParameter, string>>>();

        A.CallTo(() => _mockTransactionManager.BeginTransactionAsync(A<CancellationToken>._))
            .Returns(Task.FromResult(_mockTransaction));

        _behavior = new TransactionBehavior<TestUseCaseParameter, string>(_mockTransactionManager, _mockLogger);
    }

    [Fact]
    public void Constructor_WithNullTransactionManager_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new TransactionBehavior<TestUseCaseParameter, string>(null!, _mockLogger));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new TransactionBehavior<TestUseCaseParameter, string>(_mockTransactionManager, null!));
    }

    [Fact]
    public async Task ExecuteAsync_OnSuccessfulExecution_ShouldCommitTransaction()
    {
        // Arrange
        var parameter = new TestUseCaseParameter();
        var expectedResult = Execution.Success("Test Result");
        var nextDelegate = A.Fake<PipelineBehaviorDelegate<string>>();
        A.CallTo(() => nextDelegate()).Returns(Task.FromResult(expectedResult));

        // Act
        var result = await _behavior.ExecuteAsync(parameter, nextDelegate);

        // Assert
        result.ExecutionSucceeded.ShouldBeTrue();
        result.CheckedValue.ShouldBe("Test Result");

        // Verify transaction lifecycle
        A.CallTo(() => _mockTransactionManager.BeginTransactionAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _mockTransaction.CommitAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _mockTransaction.RollbackAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _mockTransaction.Dispose())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_OnFailedExecution_ShouldRollbackTransaction()
    {
        // Arrange
        var parameter = new TestUseCaseParameter();
        var expectedResult = Execution.Failure<string>("Use case failed");
        var nextDelegate = A.Fake<PipelineBehaviorDelegate<string>>();
        A.CallTo(() => nextDelegate()).Returns(Task.FromResult(expectedResult));

        // Act
        var result = await _behavior.ExecuteAsync(parameter, nextDelegate);

        // Assert
        result.ExecutionSucceeded.ShouldBeFalse();
        result.Error?.Message.ShouldBe("Use case failed");

        // Verify transaction lifecycle
        A.CallTo(() => _mockTransactionManager.BeginTransactionAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _mockTransaction.RollbackAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _mockTransaction.CommitAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _mockTransaction.Dispose())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_OnException_ShouldRollbackTransactionAndReturnFailure()
    {
        // Arrange
        var parameter = new TestUseCaseParameter();
        var expectedException = new InvalidOperationException("Test exception");
        var nextDelegate = A.Fake<PipelineBehaviorDelegate<string>>();
        A.CallTo(() => nextDelegate()).Throws(expectedException);

        // Act
        var result = await _behavior.ExecuteAsync(parameter, nextDelegate);

        // Assert
        result.ExecutionSucceeded.ShouldBeFalse();
        result.Error?.Message.ShouldContain("Exception in TransactionBehavior");
        result.Error?.Message.ShouldContain("Test exception");

        // Verify transaction lifecycle
        A.CallTo(() => _mockTransactionManager.BeginTransactionAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _mockTransaction.RollbackAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _mockTransaction.CommitAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _mockTransaction.Dispose())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_WhenRollbackFails_ShouldStillReturnOriginalFailure()
    {
        // Arrange
        var parameter = new TestUseCaseParameter();
        var originalException = new InvalidOperationException("Original exception");
        var rollbackException = new InvalidOperationException("Rollback failed");

        var nextDelegate = A.Fake<PipelineBehaviorDelegate<string>>();
        A.CallTo(() => nextDelegate()).Throws(originalException);
        A.CallTo(() => _mockTransaction.RollbackAsync(A<CancellationToken>._)).Throws(rollbackException);

        // Act
        var result = await _behavior.ExecuteAsync(parameter, nextDelegate);

        // Assert
        result.ExecutionSucceeded.ShouldBeFalse();
        result.Error?.Message.ShouldContain("Exception in TransactionBehavior");
        result.Error?.Message.ShouldContain("Original exception"); // Should preserve original exception message

        // Verify transaction lifecycle
        A.CallTo(() => _mockTransactionManager.BeginTransactionAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _mockTransaction.RollbackAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _mockTransaction.CommitAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _mockTransaction.Dispose())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_WhenTransactionManagerFails_ShouldReturnFailure()
    {
        // Arrange
        var parameter = new TestUseCaseParameter();
        var transactionException = new InvalidOperationException("Failed to create transaction");

        A.CallTo(() => _mockTransactionManager.BeginTransactionAsync(A<CancellationToken>._))
            .Throws(transactionException);

        var nextDelegate = A.Fake<PipelineBehaviorDelegate<string>>();

        // Act
        var result = await _behavior.ExecuteAsync(parameter, nextDelegate);

        // Assert
        result.ExecutionSucceeded.ShouldBeFalse();
        result.Error?.Message.ShouldContain("Exception in TransactionBehavior");
        result.Error?.Message.ShouldContain("Failed to create transaction");

        // Verify transaction manager was called but next wasn't
        A.CallTo(() => _mockTransactionManager.BeginTransactionAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => nextDelegate()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_ShouldPassTokenToAllMethods()
    {
        // Arrange
        var parameter = new TestUseCaseParameter();
        var expectedResult = Execution.Success("Test Result");
        var nextDelegate = A.Fake<PipelineBehaviorDelegate<string>>();
        A.CallTo(() => nextDelegate()).Returns(Task.FromResult(expectedResult));
        var cancellationToken = new CancellationToken();

        // Act
        var result = await _behavior.ExecuteAsync(parameter, nextDelegate, cancellationToken);

        // Assert
        result.ExecutionSucceeded.ShouldBeTrue();

        // Verify cancellation token was passed to transaction methods
        A.CallTo(() => _mockTransactionManager.BeginTransactionAsync(cancellationToken))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _mockTransaction.CommitAsync(cancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_OnFailure_ShouldPassCancellationTokenToRollback()
    {
        // Arrange
        var parameter = new TestUseCaseParameter();
        var expectedResult = Execution.Failure<string>("Use case failed");
        var nextDelegate = A.Fake<PipelineBehaviorDelegate<string>>();
        A.CallTo(() => nextDelegate()).Returns(Task.FromResult(expectedResult));
        var cancellationToken = new CancellationToken();

        // Act
        var result = await _behavior.ExecuteAsync(parameter, nextDelegate, cancellationToken);

        // Assert
        result.ExecutionSucceeded.ShouldBeFalse();

        // Verify cancellation token was passed to rollback
        A.CallTo(() => _mockTransactionManager.BeginTransactionAsync(cancellationToken))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _mockTransaction.RollbackAsync(cancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    // Test helper classes
    public class TestUseCaseParameter : IUseCaseParameter<string>
    {
    }
}