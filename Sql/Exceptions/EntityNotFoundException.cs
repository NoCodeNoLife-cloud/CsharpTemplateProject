namespace Sql.Exceptions;

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