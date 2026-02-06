namespace Sql.Interfaces;

/// <summary>
/// Generic CRUD operations interface
/// 通用CRUD操作接口
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
/// <typeparam name="TKey">Primary key type</typeparam>
public interface ICrudService<T, TKey> where T : class where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Creates a new entity
    /// 创建实体
    /// </summary>
    /// <param name="entity">The entity to create</param>
    /// <returns>The created entity</returns>
    T Create(T entity);

    /// <summary>
    /// Asynchronously creates a new entity
    /// 异步创建实体
    /// </summary>
    /// <param name="entity">The entity to create</param>
    /// <returns>Task containing the created entity</returns>
    Task<T> CreateAsync(T entity);

    /// <summary>
    /// Gets an entity by its primary key
    /// 根据主键获取实体
    /// </summary>
    /// <param name="id">Entity primary key</param>
    /// <returns>The found entity, or null if not found</returns>
    T? GetById(TKey id);

    /// <summary>
    /// Asynchronously gets an entity by its primary key
    /// 异步根据主键获取实体
    /// </summary>
    /// <param name="id">Entity primary key</param>
    /// <returns>Task containing the found entity, or null if not found</returns>
    Task<T?> GetByIdAsync(TKey id);

    /// <summary>
    /// Gets all entities
    /// 获取所有实体
    /// </summary>
    /// <returns>Collection of all entities</returns>
    IEnumerable<T> GetAll();

    /// <summary>
    /// Asynchronously gets all entities
    /// 异步获取所有实体
    /// </summary>
    /// <returns>Task containing collection of all entities</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Updates an existing entity
    /// 更新实体
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>The updated entity</returns>
    T Update(T entity);

    /// <summary>
    /// Asynchronously updates an existing entity
    /// 异步更新实体
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>Task containing the updated entity</returns>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity by its primary key
    /// 删除实体
    /// </summary>
    /// <param name="id">Entity primary key</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    bool Delete(TKey id);

    /// <summary>
    /// Asynchronously deletes an entity by its primary key
    /// 异步删除实体
    /// </summary>
    /// <param name="id">Entity primary key</param>
    /// <returns>Task containing true if deletion was successful, false otherwise</returns>
    Task<bool> DeleteAsync(TKey id);

    /// <summary>
    /// Creates multiple entities in batch
    /// 批量创建实体
    /// </summary>
    /// <param name="entities">Collection of entities to create</param>
    /// <returns>Collection of created entities</returns>
    IEnumerable<T> CreateBatch(IEnumerable<T> entities);

    /// <summary>
    /// Asynchronously creates multiple entities in batch
    /// 异步批量创建实体
    /// </summary>
    /// <param name="entities">Collection of entities to create</param>
    /// <returns>Task containing collection of created entities</returns>
    Task<IEnumerable<T>> CreateBatchAsync(IEnumerable<T> entities);

    /// <summary>
    /// Updates multiple entities in batch
    /// 批量更新实体
    /// </summary>
    /// <param name="entities">Collection of entities to update</param>
    /// <returns>Collection of updated entities</returns>
    IEnumerable<T> UpdateBatch(IEnumerable<T> entities);

    /// <summary>
    /// Asynchronously updates multiple entities in batch
    /// 异步批量更新实体
    /// </summary>
    /// <param name="entities">Collection of entities to update</param>
    /// <returns>Task containing collection of updated entities</returns>
    Task<IEnumerable<T>> UpdateBatchAsync(IEnumerable<T> entities);

    /// <summary>
    /// Deletes multiple entities in batch
    /// 批量删除实体
    /// </summary>
    /// <param name="ids">Collection of entity primary keys to delete</param>
    /// <returns>Number of successfully deleted entities</returns>
    int DeleteBatch(IEnumerable<TKey> ids);

    /// <summary>
    /// Asynchronously deletes multiple entities in batch
    /// 异步批量删除实体
    /// </summary>
    /// <param name="ids">Collection of entity primary keys to delete</param>
    /// <returns>Task containing number of successfully deleted entities</returns>
    Task<int> DeleteBatchAsync(IEnumerable<TKey> ids);

    /// <summary>
    /// Checks if an entity exists by its primary key
    /// 检查实体是否存在
    /// </summary>
    /// <param name="id">Entity primary key</param>
    /// <returns>True if entity exists, false otherwise</returns>
    bool Exists(TKey id);

    /// <summary>
    /// Asynchronously checks if an entity exists by its primary key
    /// 异步检查实体是否存在
    /// </summary>
    /// <param name="id">Entity primary key</param>
    /// <returns>Task containing true if entity exists, false otherwise</returns>
    Task<bool> ExistsAsync(TKey id);

    /// <summary>
    /// Gets the count of entities
    /// 获取实体数量
    /// </summary>
    /// <returns>Total number of entities</returns>
    int Count();

    /// <summary>
    /// Asynchronously gets the count of entities
    /// 异步获取实体数量
    /// </summary>
    /// <returns>Task containing total number of entities</returns>
    Task<int> CountAsync();

    /// <summary>
    /// Gets entities by condition
    /// 根据条件获取实体
    /// </summary>
    /// <param name="condition">Query condition</param>
    /// <param name="parameters">Query parameters</param>
    /// <returns>Collection of entities matching the condition</returns>
    IEnumerable<T> GetByCondition(string condition, object? parameters = null);

    /// <summary>
    /// Asynchronously gets entities by condition
    /// 异步根据条件获取实体
    /// </summary>
    /// <param name="condition">Query condition</param>
    /// <param name="parameters">Query parameters</param>
    /// <returns>Task containing collection of entities matching the condition</returns>
    Task<IEnumerable<T>> GetByConditionAsync(string condition, object? parameters = null);
}