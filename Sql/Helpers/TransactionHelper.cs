using MySqlConnector;
using Sql.Exceptions;

namespace Sql.Helpers;

/// <summary>
/// Transaction management helper class
/// </summary>
public class TransactionHelper : IDisposable
{
    private readonly DatabaseConnectionManager _connectionManager;
    private MySqlTransaction? _transaction;
    private bool _disposed;
    private bool _completed;

    /// <summary>
    /// Initializes a new instance of the TransactionHelper class
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    public TransactionHelper(string connectionString)
    {
        _connectionManager = new DatabaseConnectionManager(connectionString);
    }

    /// <summary>
    /// Begins a transaction
    /// </summary>
    /// <exception cref="TransactionException">Thrown when transaction is already active or connection fails</exception>
    public void BeginTransaction()
    {
        if (_transaction != null)
        {
            throw new TransactionException("Transaction is already active");
        }

        try
        {
            var connection = _connectionManager.GetConnection();
            _transaction = connection.BeginTransaction();
        }
        catch (MySqlException ex)
        {
            throw new TransactionException($"Failed to begin transaction: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously begins a transaction
    /// </summary>
    /// <returns>Task</returns>
    /// <exception cref="TransactionException">Thrown when transaction is already active or connection fails</exception>
    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            throw new TransactionException("Transaction is already active");
        }

        try
        {
            var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            _transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);
        }
        catch (MySqlException ex)
        {
            throw new TransactionException($"Failed to begin transaction: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Commits the transaction
    /// </summary>
    /// <exception cref="TransactionException">Thrown when there is no active transaction</exception>
    public void Commit()
    {
        if (_transaction == null)
        {
            throw new TransactionException("No active transaction to commit");
        }

        if (_completed)
        {
            throw new TransactionException("Transaction has already been completed");
        }

        try
        {
            _transaction.Commit();
            _completed = true;
        }
        catch (MySqlException ex)
        {
            throw new TransactionException($"Failed to commit transaction: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously commits the transaction
    /// </summary>
    /// <returns>Task</returns>
    /// <exception cref="TransactionException">Thrown when there is no active transaction</exception>
    public async Task CommitAsync()
    {
        if (_transaction == null)
        {
            throw new TransactionException("No active transaction to commit");
        }

        if (_completed)
        {
            throw new TransactionException("Transaction has already been completed");
        }

        try
        {
            await _transaction.CommitAsync().ConfigureAwait(false);
            _completed = true;
        }
        catch (MySqlException ex)
        {
            throw new TransactionException($"Failed to commit transaction: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Rolls back the transaction
    /// </summary>
    /// <exception cref="TransactionException">Thrown when there is no active transaction</exception>
    public void Rollback()
    {
        if (_transaction == null)
        {
            throw new TransactionException("No active transaction to rollback");
        }

        if (_completed)
        {
            throw new TransactionException("Transaction has already been completed");
        }

        try
        {
            _transaction.Rollback();
            _completed = true;
        }
        catch (MySqlException ex)
        {
            throw new TransactionException($"Failed to rollback transaction: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously rolls back the transaction
    /// </summary>
    /// <returns>Task</returns>
    /// <exception cref="TransactionException">Thrown when there is no active transaction</exception>
    public async Task RollbackAsync()
    {
        if (_transaction == null)
        {
            throw new TransactionException("No active transaction to rollback");
        }

        if (_completed)
        {
            throw new TransactionException("Transaction has already been completed");
        }

        try
        {
            await _transaction.RollbackAsync().ConfigureAwait(false);
            _completed = true;
        }
        catch (MySqlException ex)
        {
            throw new TransactionException($"Failed to rollback transaction: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets the current transaction
    /// </summary>
    /// <returns>Current transaction object</returns>
    /// <exception cref="TransactionException">Thrown when there is no active transaction</exception>
    public MySqlTransaction GetTransaction()
    {
        if (_transaction == null)
        {
            throw new TransactionException("No active transaction");
        }

        return _transaction;
    }

    /// <summary>
    /// Checks if there is an active transaction
    /// </summary>
    /// <returns>Returns true if there is an active transaction, otherwise false</returns>
    public bool HasActiveTransaction()
    {
        return _transaction != null && !_completed;
    }

    /// <summary>
    /// Executes an action within a transaction
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <exception cref="ArgumentNullException">Thrown when action is null</exception>
    /// <exception cref="TransactionException">Thrown when transaction operation fails</exception>
    public void ExecuteInTransaction(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        BeginTransaction();
        try
        {
            action();
            Commit();
        }
        catch
        {
            Rollback();
            throw;
        }
    }

    /// <summary>
    /// Asynchronously executes an action within a transaction
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <returns>Task</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null</exception>
    /// <exception cref="TransactionException">Thrown when transaction operation fails</exception>
    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        await BeginTransactionAsync().ConfigureAwait(false);
        try
        {
            await action().ConfigureAwait(false);
            await CommitAsync().ConfigureAwait(false);
        }
        catch
        {
            await RollbackAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Executes a function within a transaction and returns a result
    /// </summary>
    /// <typeparam name="TResult">Return result type</typeparam>
    /// <param name="func">Function to execute</param>
    /// <returns>Execution result</returns>
    /// <exception cref="ArgumentNullException">Thrown when func is null</exception>
    /// <exception cref="TransactionException">Thrown when transaction operation fails</exception>
    public TResult ExecuteInTransaction<TResult>(Func<TResult> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        BeginTransaction();
        try
        {
            var result = func();
            Commit();
            return result;
        }
        catch
        {
            Rollback();
            throw;
        }
    }

    /// <summary>
    /// Asynchronously executes a function within a transaction and returns a result
    /// </summary>
    /// <typeparam name="TResult">Return result type</typeparam>
    /// <param name="func">Function to execute</param>
    /// <returns>Task of execution result</returns>
    /// <exception cref="ArgumentNullException">Thrown when func is null</exception>
    /// <exception cref="TransactionException">Thrown when transaction operation fails</exception>
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        await BeginTransactionAsync().ConfigureAwait(false);
        try
        {
            var result = await func().ConfigureAwait(false);
            await CommitAsync().ConfigureAwait(false);
            return result;
        }
        catch
        {
            await RollbackAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Performs cleanup operations
    /// </summary>
    /// <param name="disposing">Whether managed resources are being disposed</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            try
            {
                if (_transaction != null && !_completed)
                {
                    _transaction.Rollback();
                }

                _transaction?.Dispose();
                _connectionManager.Dispose();
            }
            catch
            {
                // Ignore exceptions during cleanup
            }
        }

        _disposed = true;
    }

    /// <summary>
    /// Releases all resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer
    /// </summary>
    ~TransactionHelper()
    {
        Dispose(false);
    }
}