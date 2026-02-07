namespace Sql.Exceptions;

/// <summary>
/// Exception thrown when transaction operations fail
/// </summary>
public class TransactionException : DatabaseException
{
    /// <summary>
    /// Initializes a new instance of the TransactionException class
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="innerException">The inner exception</param>
    public TransactionException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}