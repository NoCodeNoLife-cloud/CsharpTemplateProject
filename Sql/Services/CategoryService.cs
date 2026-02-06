using MySqlConnector;
using Sql.Exceptions;
using Sql.Helpers;
using Sql.Interfaces;
using Sql.Models;

namespace Sql.Services;

/// <summary>
/// MySQL Category service implementation
/// </summary>
public class CategoryService : ICrudService<Category, int>, ITransactionService, IDisposable
{
    private readonly DatabaseConnectionManager _connectionManager;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the CategoryService class
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    public CategoryService(string connectionString)
    {
        _connectionManager = new DatabaseConnectionManager(connectionString);
    }

    /// <summary>
    /// Creates a new category
    /// </summary>
    /// <param name="category">Category to create</param>
    /// <returns>Created category</returns>
    /// <exception cref="ArgumentNullException">Thrown when category is null</exception>
    /// <exception cref="DuplicateKeyException">Thrown when category name already exists</exception>
    public Category Create(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        const string sql = @"
                INSERT INTO categories (name, description, parent_id, is_enabled, sort_order, created_at, updated_at)
                VALUES (@Name, @Description, @ParentId, @IsEnabled, @SortOrder, @CreatedAt, @UpdatedAt);
                SELECT LAST_INSERT_ID();";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Name", category.Name);
            command.Parameters.AddWithValue("@Description", category.Description ?? string.Empty);
            command.Parameters.AddWithValue("@ParentId", category.ParentId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsEnabled", category.IsEnabled);
            command.Parameters.AddWithValue("@SortOrder", category.SortOrder);
            command.Parameters.AddWithValue("@CreatedAt", category.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", category.UpdatedAt);

            var id = Convert.ToInt32(command.ExecuteScalar());
            category.Id = id;
            return category;
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException($"Category name '{category.Name}' already exists", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to create category: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously creates a new category
    /// </summary>
    /// <param name="category">Category to create</param>
    /// <returns>Task of created category</returns>
    public async Task<Category> CreateAsync(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        const string sql = @"
                INSERT INTO categories (name, description, parent_id, is_enabled, sort_order, created_at, updated_at)
                VALUES (@Name, @Description, @ParentId, @IsEnabled, @SortOrder, @CreatedAt, @UpdatedAt);
                SELECT LAST_INSERT_ID();";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Name", category.Name);
            command.Parameters.AddWithValue("@Description", category.Description ?? string.Empty);
            command.Parameters.AddWithValue("@ParentId", category.ParentId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsEnabled", category.IsEnabled);
            command.Parameters.AddWithValue("@SortOrder", category.SortOrder);
            command.Parameters.AddWithValue("@CreatedAt", category.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", category.UpdatedAt);

            var id = Convert.ToInt32(await command.ExecuteScalarAsync().ConfigureAwait(false));
            category.Id = id;
            return category;
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException($"Category name '{category.Name}' already exists", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to create category: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets a category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Found category, or null if not exists</returns>
    public Category? GetById(int id)
    {
        const string sql = @"
                SELECT id, name, description, parent_id, is_enabled, sort_order, created_at, updated_at
                FROM categories
                WHERE id = @Id AND is_enabled = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapCategoryFromReader(reader) : null;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get category by ID {id}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously gets a category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Task of found category, or null if not exists</returns>
    public async Task<Category?> GetByIdAsync(int id)
    {
        const string sql = @"
                SELECT id, name, description, parent_id, is_enabled, sort_order, created_at, updated_at
                FROM categories
                WHERE id = @Id AND is_enabled = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            return reader.Read() ? MapCategoryFromReader(reader) : null;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get category by ID {id}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets all categories
    /// </summary>
    /// <returns>Collection of all categories</returns>
    public IEnumerable<Category> GetAll()
    {
        const string sql = @"
                SELECT id, name, description, parent_id, is_enabled, sort_order, created_at, updated_at
                FROM categories
                WHERE is_enabled = 1
                ORDER BY sort_order ASC, created_at DESC";

        var categories = new List<Category>();

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                categories.Add(MapCategoryFromReader(reader));
            }

            return categories;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get all categories: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously gets all categories
    /// </summary>
    /// <returns>Task of all categories collection</returns>
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        const string sql = @"
                SELECT id, name, description, parent_id, is_enabled, sort_order, created_at, updated_at
                FROM categories
                WHERE is_enabled = 1
                ORDER BY sort_order ASC, created_at DESC";

        var categories = new List<Category>();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                categories.Add(MapCategoryFromReader(reader));
            }

            return categories;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get all categories: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Updates a category
    /// </summary>
    /// <param name="category">Category to update</param>
    /// <returns>Updated category</returns>
    /// <exception cref="ArgumentNullException">Thrown when category is null</exception>
    /// <exception cref="EntityNotFoundException">Thrown when category does not exist</exception>
    public Category Update(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        const string sql = @"
                UPDATE categories
                SET name = @Name,
                    description = @Description,
                    parent_id = @ParentId,
                    is_enabled = @IsEnabled,
                    sort_order = @SortOrder,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_enabled = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Name", category.Name);
            command.Parameters.AddWithValue("@Description", category.Description ?? string.Empty);
            command.Parameters.AddWithValue("@ParentId", category.ParentId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsEnabled", category.IsEnabled);
            command.Parameters.AddWithValue("@SortOrder", category.SortOrder);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@Id", category.Id);

            var affectedRows = command.ExecuteNonQuery();
            if (affectedRows == 0)
            {
                throw new EntityNotFoundException(nameof(Category), category.Id);
            }

            return category;
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException($"Category name '{category.Name}' already exists", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to update category: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously updates a category
    /// </summary>
    /// <param name="category">Category to update</param>
    /// <returns>Task of updated category</returns>
    public async Task<Category> UpdateAsync(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        const string sql = @"
                UPDATE categories
                SET name = @Name,
                    description = @Description,
                    parent_id = @ParentId,
                    is_enabled = @IsEnabled,
                    sort_order = @SortOrder,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_enabled = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Name", category.Name);
            command.Parameters.AddWithValue("@Description", category.Description ?? string.Empty);
            command.Parameters.AddWithValue("@ParentId", category.ParentId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsEnabled", category.IsEnabled);
            command.Parameters.AddWithValue("@SortOrder", category.SortOrder);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@Id", category.Id);

            var affectedRows = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            if (affectedRows == 0)
            {
                throw new EntityNotFoundException(nameof(Category), category.Id);
            }

            return category;
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException($"Category name '{category.Name}' already exists", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to update category: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deletes a category (soft delete)
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>True if deletion successful, false otherwise</returns>
    public bool Delete(int id)
    {
        const string sql = @"
                UPDATE categories
                SET is_enabled = 0,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_enabled = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@Id", id);

            var affectedRows = command.ExecuteNonQuery();
            return affectedRows > 0;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to delete category: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously deletes a category (soft delete)
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Task of true if deletion successful, false otherwise</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = @"
                UPDATE categories
                SET is_enabled = 0,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_enabled = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@Id", id);

            var affectedRows = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return affectedRows > 0;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to delete category: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates multiple categories in batch
    /// </summary>
    /// <param name="categories">Collection of categories to create</param>
    /// <returns>Collection of created categories</returns>
    public IEnumerable<Category> CreateBatch(IEnumerable<Category> categories)
    {
        if (categories == null)
            throw new ArgumentNullException(nameof(categories));

        var categoryList = categories.ToList();
        if (!categoryList.Any())
            return categoryList;

        const string sql = @"
                INSERT INTO categories (name, description, parent_id, is_enabled, sort_order, created_at, updated_at)
                VALUES (@Name, @Description, @ParentId, @IsEnabled, @SortOrder, @CreatedAt, @UpdatedAt)";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var category in categoryList)
                {
                    using var command = new MySqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@Name", category.Name);
                    command.Parameters.AddWithValue("@Description", category.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@ParentId", category.ParentId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@IsEnabled", category.IsEnabled);
                    command.Parameters.AddWithValue("@SortOrder", category.SortOrder);
                    command.Parameters.AddWithValue("@CreatedAt", category.CreatedAt);
                    command.Parameters.AddWithValue("@UpdatedAt", category.UpdatedAt);

                    command.ExecuteNonQuery();
                    category.Id = (int)command.LastInsertedId;
                }

                transaction.Commit();
                return categoryList;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException("One or more category names already exist", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to create categories in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously creates multiple categories in batch
    /// </summary>
    /// <param name="categories">Collection of categories to create</param>
    /// <returns>Task of created categories collection</returns>
    public async Task<IEnumerable<Category>> CreateBatchAsync(IEnumerable<Category> categories)
    {
        if (categories == null)
            throw new ArgumentNullException(nameof(categories));

        var categoryList = categories.ToList();
        if (!categoryList.Any())
            return categoryList;

        const string sql = @"
                INSERT INTO categories (name, description, parent_id, is_enabled, sort_order, created_at, updated_at)
                VALUES (@Name, @Description, @ParentId, @IsEnabled, @SortOrder, @CreatedAt, @UpdatedAt)";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);

            try
            {
                foreach (var category in categoryList)
                {
                    using var command = new MySqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@Name", category.Name);
                    command.Parameters.AddWithValue("@Description", category.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@ParentId", category.ParentId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@IsEnabled", category.IsEnabled);
                    command.Parameters.AddWithValue("@SortOrder", category.SortOrder);
                    command.Parameters.AddWithValue("@CreatedAt", category.CreatedAt);
                    command.Parameters.AddWithValue("@UpdatedAt", category.UpdatedAt);

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    category.Id = (int)command.LastInsertedId;
                }

                await transaction.CommitAsync().ConfigureAwait(false);
                return categoryList;
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException("One or more category names already exist", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to create categories in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Updates multiple categories in batch
    /// </summary>
    /// <param name="categories">Collection of categories to update</param>
    /// <returns>Collection of updated categories</returns>
    public IEnumerable<Category> UpdateBatch(IEnumerable<Category> categories)
    {
        if (categories == null)
            throw new ArgumentNullException(nameof(categories));

        var categoryList = categories.ToList();
        if (!categoryList.Any())
            return categoryList;

        const string sql = @"
                UPDATE categories
                SET name = @Name,
                    description = @Description,
                    parent_id = @ParentId,
                    is_enabled = @IsEnabled,
                    sort_order = @SortOrder,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_enabled = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var category in categoryList)
                {
                    using var command = new MySqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@Name", category.Name);
                    command.Parameters.AddWithValue("@Description", category.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@ParentId", category.ParentId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@IsEnabled", category.IsEnabled);
                    command.Parameters.AddWithValue("@SortOrder", category.SortOrder);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@Id", category.Id);

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
                return categoryList;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException("One or more category names already exist", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to update categories in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously updates multiple categories in batch
    /// </summary>
    /// <param name="categories">Collection of categories to update</param>
    /// <returns>Task of updated categories collection</returns>
    public async Task<IEnumerable<Category>> UpdateBatchAsync(IEnumerable<Category> categories)
    {
        if (categories == null)
            throw new ArgumentNullException(nameof(categories));

        var categoryList = categories.ToList();
        if (!categoryList.Any())
            return categoryList;

        const string sql = @"
                UPDATE categories
                SET name = @Name,
                    description = @Description,
                    parent_id = @ParentId,
                    is_enabled = @IsEnabled,
                    sort_order = @SortOrder,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_enabled = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);

            try
            {
                foreach (var category in categoryList)
                {
                    using var command = new MySqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@Name", category.Name);
                    command.Parameters.AddWithValue("@Description", category.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@ParentId", category.ParentId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@IsEnabled", category.IsEnabled);
                    command.Parameters.AddWithValue("@SortOrder", category.SortOrder);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@Id", category.Id);

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                await transaction.CommitAsync().ConfigureAwait(false);
                return categoryList;
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException("One or more category names already exist", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to update categories in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deletes multiple categories in batch
    /// </summary>
    /// <param name="ids">Collection of category IDs to delete</param>
    /// <returns>Number of successfully deleted items</returns>
    public int DeleteBatch(IEnumerable<int> ids)
    {
        if (ids == null)
            throw new ArgumentNullException(nameof(ids));

        var idList = ids.ToList();
        if (!idList.Any())
            return 0;

        const string sql = @"
                UPDATE categories
                SET is_enabled = 0,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_enabled = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                int deletedCount = 0;
                foreach (var id in idList)
                {
                    using var command = new MySqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@Id", id);

                    deletedCount += command.ExecuteNonQuery();
                }

                transaction.Commit();
                return deletedCount;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to delete categories in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously deletes multiple categories in batch
    /// </summary>
    /// <param name="ids">Collection of category IDs to delete</param>
    /// <returns>Task of number of successfully deleted items</returns>
    public async Task<int> DeleteBatchAsync(IEnumerable<int> ids)
    {
        if (ids == null)
            throw new ArgumentNullException(nameof(ids));

        var idList = ids.ToList();
        if (!idList.Any())
            return 0;

        const string sql = @"
                UPDATE categories
                SET is_enabled = 0,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_enabled = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);

            try
            {
                int deletedCount = 0;
                foreach (var id in idList)
                {
                    using var command = new MySqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@Id", id);

                    deletedCount += await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                await transaction.CommitAsync().ConfigureAwait(false);
                return deletedCount;
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to delete categories in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks if a category exists
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>True if exists, false otherwise</returns>
    public bool Exists(int id)
    {
        const string sql = "SELECT COUNT(1) FROM categories WHERE id = @Id AND is_enabled = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            var count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to check if category exists: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously checks if a category exists
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Task of true if exists, false otherwise</returns>
    public async Task<bool> ExistsAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM categories WHERE id = @Id AND is_enabled = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync().ConfigureAwait(false));
            return count > 0;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to check if category exists: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets the count of categories
    /// </summary>
    /// <returns>Total number of categories</returns>
    public int Count()
    {
        const string sql = "SELECT COUNT(1) FROM categories WHERE is_enabled = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);

            return Convert.ToInt32(command.ExecuteScalar());
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get category count: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously gets the count of categories
    /// </summary>
    /// <returns>Task of total number of categories</returns>
    public async Task<int> CountAsync()
    {
        const string sql = "SELECT COUNT(1) FROM categories WHERE is_enabled = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);

            return Convert.ToInt32(await command.ExecuteScalarAsync().ConfigureAwait(false));
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get category count: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets categories by condition
    /// </summary>
    /// <param name="condition">Query condition</param>
    /// <param name="parameters">Query parameters</param>
    /// <returns>Collection of categories matching the condition</returns>
    public IEnumerable<Category> GetByCondition(string condition, object? parameters = null)
    {
        var sql = $@"
                SELECT id, name, description, parent_id, is_enabled, sort_order, created_at, updated_at
                FROM categories
                WHERE is_enabled = 1 AND ({condition})
                ORDER BY sort_order ASC, created_at DESC";

        var categories = new List<Category>();

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);

            if (parameters != null)
            {
                var paramDict = parameters.GetType().GetProperties()
                    .ToDictionary(p => p.Name, p => p.GetValue(parameters));

                foreach (var param in paramDict)
                {
                    command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                }
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(MapCategoryFromReader(reader));
            }

            return categories;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get categories by condition: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously gets categories by condition
    /// </summary>
    /// <param name="condition">Query condition</param>
    /// <param name="parameters">Query parameters</param>
    /// <returns>Task of categories collection matching the condition</returns>
    public async Task<IEnumerable<Category>> GetByConditionAsync(string condition, object? parameters = null)
    {
        var sql = $@"
                SELECT id, name, description, parent_id, is_enabled, sort_order, created_at, updated_at
                FROM categories
                WHERE is_enabled = 1 AND ({condition})
                ORDER BY sort_order ASC, created_at DESC";

        var categories = new List<Category>();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);

            if (parameters != null)
            {
                var paramDict = parameters.GetType().GetProperties()
                    .ToDictionary(p => p.Name, p => p.GetValue(parameters));

                foreach (var param in paramDict)
                {
                    command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                }
            }

            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                categories.Add(MapCategoryFromReader(reader));
            }

            return categories;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get categories by condition: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Maps Category object from DataReader
    /// </summary>
    /// <param name="reader">DataReader object</param>
    /// <returns>Mapped category object</returns>
    private static Category MapCategoryFromReader(MySqlDataReader reader)
    {
        return new Category
        {
            Id = reader.GetInt32("id"),
            Name = reader.GetString("name"),
            Description = reader.IsDBNull("description") ? string.Empty : reader.GetString("description"),
            ParentId = reader.IsDBNull("parent_id") ? null : reader.GetInt32("parent_id"),
            IsEnabled = reader.GetBoolean("is_enabled"),
            SortOrder = reader.GetInt32("sort_order"),
            CreatedAt = reader.GetDateTime("created_at"),
            UpdatedAt = reader.GetDateTime("updated_at")
        };
    }

    /// <summary>
    /// Begins a transaction
    /// </summary>
    /// <returns>Transaction object</returns>
    public object BeginTransaction()
    {
        var connection = _connectionManager.GetConnection();
        return connection.BeginTransaction();
    }

    /// <summary>
    /// Asynchronously begins a transaction
    /// </summary>
    /// <returns>Task of transaction object</returns>
    public async Task<object> BeginTransactionAsync()
    {
        var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
        return await connection.BeginTransactionAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Commits a transaction
    /// </summary>
    /// <param name="transaction">Transaction object</param>
    public void CommitTransaction(object transaction)
    {
        if (transaction is MySqlTransaction mysqlTransaction)
        {
            mysqlTransaction.Commit();
        }
        else
        {
            throw new ArgumentException("Invalid transaction type", nameof(transaction));
        }
    }

    /// <summary>
    /// Asynchronously commits a transaction
    /// </summary>
    /// <param name="transaction">Transaction object</param>
    /// <returns>Task</returns>
    public async Task CommitTransactionAsync(object transaction)
    {
        if (transaction is MySqlTransaction mysqlTransaction)
        {
            await mysqlTransaction.CommitAsync().ConfigureAwait(false);
        }
        else
        {
            throw new ArgumentException("Invalid transaction type", nameof(transaction));
        }
    }

    /// <summary>
    /// Rolls back a transaction
    /// </summary>
    /// <param name="transaction">Transaction object</param>
    public void RollbackTransaction(object transaction)
    {
        if (transaction is MySqlTransaction mysqlTransaction)
        {
            mysqlTransaction.Rollback();
        }
        else
        {
            throw new ArgumentException("Invalid transaction type", nameof(transaction));
        }
    }

    /// <summary>
    /// Asynchronously rolls back a transaction
    /// </summary>
    /// <param name="transaction">Transaction object</param>
    /// <returns>Task</returns>
    public async Task RollbackTransactionAsync(object transaction)
    {
        if (transaction is MySqlTransaction mysqlTransaction)
        {
            await mysqlTransaction.RollbackAsync().ConfigureAwait(false);
        }
        else
        {
            throw new ArgumentException("Invalid transaction type", nameof(transaction));
        }
    }

    /// <summary>
    /// Executes an action within a transaction
    /// </summary>
    /// <param name="action">Action to execute</param>
    public void ExecuteInTransaction(Action action)
    {
        using var transaction = (MySqlTransaction)BeginTransaction();
        try
        {
            action();
            CommitTransaction(transaction);
        }
        catch
        {
            RollbackTransaction(transaction);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously executes an action within a transaction
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <returns>Task</returns>
    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        var transaction = await BeginTransactionAsync().ConfigureAwait(false);
        try
        {
            await action().ConfigureAwait(false);
            await CommitTransactionAsync(transaction).ConfigureAwait(false);
        }
        catch
        {
            await RollbackTransactionAsync(transaction).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Executes a function within a transaction and returns a result
    /// </summary>
    /// <typeparam name="TResult">Return result type</typeparam>
    /// <param name="func">Function to execute</param>
    /// <returns>Execution result</returns>
    public TResult ExecuteInTransaction<TResult>(Func<TResult> func)
    {
        using var transaction = (MySqlTransaction)BeginTransaction();
        try
        {
            var result = func();
            CommitTransaction(transaction);
            return result;
        }
        catch
        {
            RollbackTransaction(transaction);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously executes a function within a transaction and returns a result
    /// </summary>
    /// <typeparam name="TResult">Return result type</typeparam>
    /// <param name="func">Function to execute</param>
    /// <returns>Task of execution result</returns>
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> func)
    {
        var transaction = await BeginTransactionAsync().ConfigureAwait(false);
        try
        {
            var result = await func().ConfigureAwait(false);
            await CommitTransactionAsync(transaction).ConfigureAwait(false);
            return result;
        }
        catch
        {
            await RollbackTransactionAsync(transaction).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Performs cleanup operations
    /// </summary>
    /// <param name="disposing">Whether disposing managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _connectionManager?.Dispose();
            }

            _disposed = true;
        }
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
    ~CategoryService()
    {
        Dispose(false);
    }
}