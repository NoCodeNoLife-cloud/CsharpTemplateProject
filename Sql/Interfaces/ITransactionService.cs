namespace Sql.Interfaces;

/// <summary>
/// Transaction operations interface
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Begins a transaction
    /// </summary>
    /// <returns>Transaction object</returns>
    object BeginTransaction();

    /// <summary>
    /// Asynchronously begins a transaction
    /// </summary>
    /// <returns>Task of transaction object</returns>
    Task<object> BeginTransactionAsync();

    /// <summary>
    /// Commits a transaction
    /// </summary>
    /// <param name="transaction">Transaction object</param>
    void CommitTransaction(object transaction);

    /// <summary>
    /// Asynchronously commits a transaction
    /// </summary>
    /// <param name="transaction">Transaction object</param>
    /// <returns>Task</returns>
    Task CommitTransactionAsync(object transaction);

    /// <summary>
    /// Rolls back a transaction
    /// </summary>
    /// <param name="transaction">Transaction object</param>
    void RollbackTransaction(object transaction);

    /// <summary>
    /// Asynchronously rolls back a transaction
    /// </summary>
    /// <param name="transaction">Transaction object</param>
    /// <returns>Task</returns>
    Task RollbackTransactionAsync(object transaction);

    /// <summary>
    /// Executes an action within a transaction
    /// </summary>
    /// <param name="action">Action to execute</param>
    void ExecuteInTransaction(Action action);

    /// <summary>
    /// Asynchronously executes an action within a transaction
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <returns>Task</returns>
    Task ExecuteInTransactionAsync(Func<Task> action);

    /// <summary>
    /// Executes a function within a transaction and returns a result
    /// </summary>
    /// <typeparam name="TResult">Return result type</typeparam>
    /// <param name="func">Function to execute</param>
    /// <returns>Execution result</returns>
    TResult ExecuteInTransaction<TResult>(Func<TResult> func);

    /// <summary>
    /// Asynchronously executes a function within a transaction and returns a result
    /// </summary>
    /// <typeparam name="TResult">Return result type</typeparam>
    /// <param name="func">Function to execute</param>
    /// <returns>Task of execution result</returns>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> func);
}