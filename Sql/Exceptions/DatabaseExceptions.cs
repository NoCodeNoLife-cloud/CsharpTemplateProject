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

/// <summary>
/// Exception thrown when entity is not found
/// </summary>
public class EntityNotFoundException : DatabaseException
{
    /// <summary>
    /// Gets the entity type
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Gets the entity identifier
    /// </summary>
    public object EntityId { get; }

    /// <summary>
    /// Initializes a new instance of the EntityNotFoundException class
    /// </summary>
    /// <param name="entityType">The entity type</param>
    /// <param name="entityId">The entity identifier</param>
    /// <param name="message">The exception message</param>
    public EntityNotFoundException(string entityType, object entityId, string? message = null)
        : base(message ?? $"Entity of type '{entityType}' with id '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

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