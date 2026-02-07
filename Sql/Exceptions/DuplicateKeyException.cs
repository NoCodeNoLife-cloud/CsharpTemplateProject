namespace Sql.Exceptions;

/// <summary>
/// Exception thrown when duplicate key constraint is violated
/// </summary>
public class DuplicateKeyException : DatabaseException
{
    /// <summary>
    /// Gets the conflicting key value
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Initializes a new instance of the DuplicateKeyException class
    /// </summary>
    /// <param name="key">The conflicting key value</param>
    /// <param name="message">The exception message</param>
    /// <param name="innerException">The inner exception</param>
    public DuplicateKeyException(string key, string? message = null, Exception? innerException = null)
        : base(message ?? $"Duplicate key violation: '{key}'", innerException)
    {
        Key = key;
    }
}