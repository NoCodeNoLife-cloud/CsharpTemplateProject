using MySqlConnector;
using Sql.Exceptions;
using Sql.Helpers;
using Sql.Interfaces;
using Sql.Models;

namespace Sql.Services;

/// <summary>
/// MySQL Product service implementation
/// </summary>
public class ProductService : ICrudService<Product, int>, ITransactionService, IDisposable
{
    private readonly DatabaseConnectionManager _connectionManager;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the ProductService class
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    public ProductService(string connectionString)
    {
        _connectionManager = new DatabaseConnectionManager(connectionString);
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="product">Product to create</param>
    /// <returns>Created product</returns>
    /// <exception cref="ArgumentNullException">Thrown when product is null</exception>
    /// <exception cref="DuplicateKeyException">Thrown when product name already exists</exception>
    public Product Create(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        const string sql = @"
                INSERT INTO products (name, description, price, stock_quantity, category_id, is_available, created_at, updated_at)
                VALUES (@Name, @Description, @Price, @StockQuantity, @CategoryId, @IsAvailable, @CreatedAt, @UpdatedAt);
                SELECT LAST_INSERT_ID();";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Name", product.Name);
            command.Parameters.AddWithValue("@Description", product.Description ?? string.Empty);
            command.Parameters.AddWithValue("@Price", product.Price);
            command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
            command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
            command.Parameters.AddWithValue("@IsAvailable", product.IsAvailable);
            command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", product.UpdatedAt);

            var id = Convert.ToInt32(command.ExecuteScalar());
            product.Id = id;
            return product;
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException($"Product name '{product.Name}' already exists", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to create product: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously creates a new product
    /// </summary>
    /// <param name="product">Product to create</param>
    /// <returns>Task of created product</returns>
    public async Task<Product> CreateAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        const string sql = @"
                INSERT INTO products (name, description, price, stock_quantity, category_id, is_available, created_at, updated_at)
                VALUES (@Name, @Description, @Price, @StockQuantity, @CategoryId, @IsAvailable, @CreatedAt, @UpdatedAt);
                SELECT LAST_INSERT_ID();";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Name", product.Name);
            command.Parameters.AddWithValue("@Description", product.Description ?? string.Empty);
            command.Parameters.AddWithValue("@Price", product.Price);
            command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
            command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
            command.Parameters.AddWithValue("@IsAvailable", product.IsAvailable);
            command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", product.UpdatedAt);

            var id = Convert.ToInt32(await command.ExecuteScalarAsync().ConfigureAwait(false));
            product.Id = id;
            return product;
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException($"Product name '{product.Name}' already exists", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to create product: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets a product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Found product, or null if not exists</returns>
    public Product? GetById(int id)
    {
        const string sql = @"
                SELECT id, name, description, price, stock_quantity, category_id, is_available, created_at, updated_at
                FROM products
                WHERE id = @Id AND is_available = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapProductFromReader(reader) : null;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get product by ID {id}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously gets a product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Task of found product, or null if not exists</returns>
    public async Task<Product?> GetByIdAsync(int id)
    {
        const string sql = @"
                SELECT id, name, description, price, stock_quantity, category_id, is_available, created_at, updated_at
                FROM products
                WHERE id = @Id AND is_available = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            return reader.Read() ? MapProductFromReader(reader) : null;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get product by ID {id}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets all products
    /// </summary>
    /// <returns>Collection of all products</returns>
    public IEnumerable<Product> GetAll()
    {
        const string sql = @"
                SELECT id, name, description, price, stock_quantity, category_id, is_available, created_at, updated_at
                FROM products
                WHERE is_available = 1
                ORDER BY created_at DESC";

        var products = new List<Product>();

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                products.Add(MapProductFromReader(reader));
            }

            return products;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get all products: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously gets all products
    /// </summary>
    /// <returns>Task of all products collection</returns>
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        const string sql = @"
                SELECT id, name, description, price, stock_quantity, category_id, is_available, created_at, updated_at
                FROM products
                WHERE is_available = 1
                ORDER BY created_at DESC";

        var products = new List<Product>();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                products.Add(MapProductFromReader(reader));
            }

            return products;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get all products: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Updates a product
    /// </summary>
    /// <param name="product">Product to update</param>
    /// <returns>Updated product</returns>
    /// <exception cref="ArgumentNullException">Thrown when product is null</exception>
    /// <exception cref="EntityNotFoundException">Thrown when product does not exist</exception>
    public Product Update(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        const string sql = @"
                UPDATE products
                SET name = @Name,
                    description = @Description,
                    price = @Price,
                    stock_quantity = @StockQuantity,
                    category_id = @CategoryId,
                    is_available = @IsAvailable,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_available = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Name", product.Name);
            command.Parameters.AddWithValue("@Description", product.Description ?? string.Empty);
            command.Parameters.AddWithValue("@Price", product.Price);
            command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
            command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
            command.Parameters.AddWithValue("@IsAvailable", product.IsAvailable);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@Id", product.Id);

            var affectedRows = command.ExecuteNonQuery();
            if (affectedRows == 0)
            {
                throw new EntityNotFoundException(nameof(Product), product.Id);
            }

            return product;
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException($"Product name '{product.Name}' already exists", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to update product: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously updates a product
    /// </summary>
    /// <param name="product">Product to update</param>
    /// <returns>Task of updated product</returns>
    public async Task<Product> UpdateAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        const string sql = @"
                UPDATE products
                SET name = @Name,
                    description = @Description,
                    price = @Price,
                    stock_quantity = @StockQuantity,
                    category_id = @CategoryId,
                    is_available = @IsAvailable,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_available = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Name", product.Name);
            command.Parameters.AddWithValue("@Description", product.Description ?? string.Empty);
            command.Parameters.AddWithValue("@Price", product.Price);
            command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
            command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
            command.Parameters.AddWithValue("@IsAvailable", product.IsAvailable);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@Id", product.Id);

            var affectedRows = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            if (affectedRows == 0)
            {
                throw new EntityNotFoundException(nameof(Product), product.Id);
            }

            return product;
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException($"Product name '{product.Name}' already exists", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to update product: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deletes a product (soft delete)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>True if deletion successful, false otherwise</returns>
    public bool Delete(int id)
    {
        const string sql = @"
                UPDATE products
                SET is_available = 0,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_available = 1";

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
            throw new QueryException($"Failed to delete product: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously deletes a product (soft delete)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Task of true if deletion successful, false otherwise</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = @"
                UPDATE products
                SET is_available = 0,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_available = 1";

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
            throw new QueryException($"Failed to delete product: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates multiple products in batch
    /// </summary>
    /// <param name="products">Collection of products to create</param>
    /// <returns>Collection of created products</returns>
    public IEnumerable<Product> CreateBatch(IEnumerable<Product> products)
    {
        if (products == null)
            throw new ArgumentNullException(nameof(products));

        var productList = products.ToList();
        if (!productList.Any())
            return productList;

        const string sql = @"
                INSERT INTO products (name, description, price, stock_quantity, category_id, is_available, created_at, updated_at)
                VALUES (@Name, @Description, @Price, @StockQuantity, @CategoryId, @IsAvailable, @CreatedAt, @UpdatedAt)";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var product in productList)
                {
                    using var command = new MySqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Description", product.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
                    command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                    command.Parameters.AddWithValue("@IsAvailable", product.IsAvailable);
                    command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
                    command.Parameters.AddWithValue("@UpdatedAt", product.UpdatedAt);

                    command.ExecuteNonQuery();
                    product.Id = (int)command.LastInsertedId;
                }

                transaction.Commit();
                return productList;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException("One or more product names already exist", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to create products in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously creates multiple products in batch
    /// </summary>
    /// <param name="products">Collection of products to create</param>
    /// <returns>Task of created products collection</returns>
    public async Task<IEnumerable<Product>> CreateBatchAsync(IEnumerable<Product> products)
    {
        if (products == null)
            throw new ArgumentNullException(nameof(products));

        var productList = products.ToList();
        if (!productList.Any())
            return productList;

        const string sql = @"
                INSERT INTO products (name, description, price, stock_quantity, category_id, is_available, created_at, updated_at)
                VALUES (@Name, @Description, @Price, @StockQuantity, @CategoryId, @IsAvailable, @CreatedAt, @UpdatedAt)";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);

            try
            {
                foreach (var product in productList)
                {
                    using var command = new MySqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Description", product.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
                    command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                    command.Parameters.AddWithValue("@IsAvailable", product.IsAvailable);
                    command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
                    command.Parameters.AddWithValue("@UpdatedAt", product.UpdatedAt);

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    product.Id = (int)command.LastInsertedId;
                }

                await transaction.CommitAsync().ConfigureAwait(false);
                return productList;
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException("One or more product names already exist", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to create products in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Updates multiple products in batch
    /// </summary>
    /// <param name="products">Collection of products to update</param>
    /// <returns>Collection of updated products</returns>
    public IEnumerable<Product> UpdateBatch(IEnumerable<Product> products)
    {
        if (products == null)
            throw new ArgumentNullException(nameof(products));

        var productList = products.ToList();
        if (!productList.Any())
            return productList;

        const string sql = @"
                UPDATE products
                SET name = @Name,
                    description = @Description,
                    price = @Price,
                    stock_quantity = @StockQuantity,
                    category_id = @CategoryId,
                    is_available = @IsAvailable,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_available = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var product in productList)
                {
                    using var command = new MySqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Description", product.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
                    command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                    command.Parameters.AddWithValue("@IsAvailable", product.IsAvailable);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@Id", product.Id);

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
                return productList;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException("One or more product names already exist", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to update products in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously updates multiple products in batch
    /// </summary>
    /// <param name="products">Collection of products to update</param>
    /// <returns>Task of updated products collection</returns>
    public async Task<IEnumerable<Product>> UpdateBatchAsync(IEnumerable<Product> products)
    {
        if (products == null)
            throw new ArgumentNullException(nameof(products));

        var productList = products.ToList();
        if (!productList.Any())
            return productList;

        const string sql = @"
                UPDATE products
                SET name = @Name,
                    description = @Description,
                    price = @Price,
                    stock_quantity = @StockQuantity,
                    category_id = @CategoryId,
                    is_available = @IsAvailable,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_available = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);

            try
            {
                foreach (var product in productList)
                {
                    using var command = new MySqlCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Description", product.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
                    command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                    command.Parameters.AddWithValue("@IsAvailable", product.IsAvailable);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@Id", product.Id);

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                await transaction.CommitAsync().ConfigureAwait(false);
                return productList;
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            throw new DuplicateKeyException("One or more product names already exist", ex);
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to update products in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deletes multiple products in batch
    /// </summary>
    /// <param name="ids">Collection of product IDs to delete</param>
    /// <returns>Number of successfully deleted items</returns>
    public int DeleteBatch(IEnumerable<int> ids)
    {
        if (ids == null)
            throw new ArgumentNullException(nameof(ids));

        var idList = ids.ToList();
        if (!idList.Any())
            return 0;

        const string sql = @"
                UPDATE products
                SET is_available = 0,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_available = 1";

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
            throw new QueryException($"Failed to delete products in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously deletes multiple products in batch
    /// </summary>
    /// <param name="ids">Collection of product IDs to delete</param>
    /// <returns>Task of number of successfully deleted items</returns>
    public async Task<int> DeleteBatchAsync(IEnumerable<int> ids)
    {
        if (ids == null)
            throw new ArgumentNullException(nameof(ids));

        var idList = ids.ToList();
        if (!idList.Any())
            return 0;

        const string sql = @"
                UPDATE products
                SET is_available = 0,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND is_available = 1";

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
            throw new QueryException($"Failed to delete products in batch: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks if a product exists
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>True if exists, false otherwise</returns>
    public bool Exists(int id)
    {
        const string sql = "SELECT COUNT(1) FROM products WHERE id = @Id AND is_available = 1";

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
            throw new QueryException($"Failed to check if product exists: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously checks if a product exists
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Task of true if exists, false otherwise</returns>
    public async Task<bool> ExistsAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM products WHERE id = @Id AND is_available = 1";

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
            throw new QueryException($"Failed to check if product exists: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets the count of products
    /// </summary>
    /// <returns>Total number of products</returns>
    public int Count()
    {
        const string sql = "SELECT COUNT(1) FROM products WHERE is_available = 1";

        try
        {
            using var connection = _connectionManager.GetConnection();
            using var command = new MySqlCommand(sql, connection);

            return Convert.ToInt32(command.ExecuteScalar());
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get product count: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously gets the count of products
    /// </summary>
    /// <returns>Task of total number of products</returns>
    public async Task<int> CountAsync()
    {
        const string sql = "SELECT COUNT(1) FROM products WHERE is_available = 1";

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync().ConfigureAwait(false);
            using var command = new MySqlCommand(sql, connection);

            return Convert.ToInt32(await command.ExecuteScalarAsync().ConfigureAwait(false));
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get product count: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets products by condition
    /// </summary>
    /// <param name="condition">Query condition</param>
    /// <param name="parameters">Query parameters</param>
    /// <returns>Collection of products matching the condition</returns>
    public IEnumerable<Product> GetByCondition(string condition, object? parameters = null)
    {
        var sql = $@"
                SELECT id, name, description, price, stock_quantity, category_id, is_available, created_at, updated_at
                FROM products
                WHERE is_available = 1 AND ({condition})
                ORDER BY created_at DESC";

        var products = new List<Product>();

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
                products.Add(MapProductFromReader(reader));
            }

            return products;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get products by condition: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously gets products by condition
    /// </summary>
    /// <param name="condition">Query condition</param>
    /// <param name="parameters">Query parameters</param>
    /// <returns>Task of products collection matching the condition</returns>
    public async Task<IEnumerable<Product>> GetByConditionAsync(string condition, object? parameters = null)
    {
        var sql = $@"
                SELECT id, name, description, price, stock_quantity, category_id, is_available, created_at, updated_at
                FROM products
                WHERE is_available = 1 AND ({condition})
                ORDER BY created_at DESC";

        var products = new List<Product>();

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
                products.Add(MapProductFromReader(reader));
            }

            return products;
        }
        catch (MySqlException ex)
        {
            throw new QueryException($"Failed to get products by condition: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Maps Product object from DataReader
    /// </summary>
    /// <param name="reader">DataReader object</param>
    /// <returns>Mapped product object</returns>
    private static Product MapProductFromReader(MySqlDataReader reader)
    {
        return new Product
        {
            Id = reader.GetInt32("id"),
            Name = reader.GetString("name"),
            Description = reader.IsDBNull("description") ? string.Empty : reader.GetString("description"),
            Price = reader.GetDecimal("price"),
            StockQuantity = reader.GetInt32("stock_quantity"),
            CategoryId = reader.GetInt32("category_id"),
            IsAvailable = reader.GetBoolean("is_available"),
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
    ~ProductService()
    {
        Dispose(false);
    }
}