namespace Sql.Exceptions;

/// <summary>
/// Exception thrown when database connection fails
/// </summary>
public class ConnectionException : DatabaseException
{
    /// <summary>
    /// Initializes a new instance of the ConnectionException class
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="innerException">The inner exception</param>
    public ConnectionException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}