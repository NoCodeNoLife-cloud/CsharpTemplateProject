namespace Sql.Exceptions;

/// <summary>
/// Exception thrown when database query fails
/// </summary>
public class QueryException : DatabaseException
{
    /// <summary>
    /// Initializes a new instance of the QueryException class
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="innerException">The inner exception</param>
    public QueryException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}