namespace Sql.Exceptions;

/// <summary>
/// Base exception class for database operations
/// </summary>
public class DatabaseException : Exception
{
    /// <summary>
    /// Initializes a new instance of the DatabaseException class
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="innerException">The inner exception</param>
    public DatabaseException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}