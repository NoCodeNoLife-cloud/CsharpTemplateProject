using Sql.Helpers;
using Sql.Models;
using Sql.Services;

namespace Sql.Examples;

/// <summary>
/// MySQL CRUD operations usage examples
/// </summary>
public class CrudExamples
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the CrudExamples class
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    public CrudExamples(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// User CRUD operations example
    /// </summary>
    public async Task UserCrudExample()
    {
        Console.WriteLine("=== User CRUD Example ===");

        var userService = new UserService(_connectionString);

        try
        {
            // Create user
            var newUser = new User
            {
                Username = "john_doe",
                Email = "john@example.com",
                PasswordHash = "hashed_password_123",
                IsActive = true
            };

            var createdUser = await userService.CreateAsync(newUser);
            Console.WriteLine($"Created user: ID={createdUser.Id}, Username={createdUser.Username}");

            // Get user
            var user = await userService.GetByIdAsync(createdUser.Id);
            if (user != null)
            {
                Console.WriteLine($"Retrieved user: ID={user.Id}, Email={user.Email}");
            }

            // Update user
            user!.Username = "john_doe_updated";
            user.Email = "john.updated@example.com";
            var updatedUser = await userService.UpdateAsync(user);
            Console.WriteLine($"Updated user: Username={updatedUser.Username}, Email={updatedUser.Email}");

            // Get all users
            var allUsers = await userService.GetAllAsync();
            Console.WriteLine($"Total users: {allUsers.Count()}");

            // Conditional query
            var activeUsers = await userService.GetByConditionAsync("is_active = @IsActive", new { IsActive = true });
            Console.WriteLine($"Active users: {activeUsers.Count()}");

            // Check existence
            var exists = await userService.ExistsAsync(createdUser.Id);
            Console.WriteLine($"User exists: {exists}");

            // Get count
            var count = await userService.CountAsync();
            Console.WriteLine($"User count: {count}");

            // Delete user
            var deleted = await userService.DeleteAsync(createdUser.Id);
            Console.WriteLine($"User deleted: {deleted}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in user CRUD example: {ex.Message}");
        }
        finally
        {
            userService.Dispose();
        }
    }

    /// <summary>
    /// Product CRUD operations example
    /// </summary>
    public async Task ProductCrudExample()
    {
        Console.WriteLine("\n=== Product CRUD Example ===");

        var productService = new ProductService(_connectionString);

        try
        {
            // Create product
            var newProduct = new Product
            {
                Name = "Laptop Computer",
                Description = "High-performance laptop for developers",
                Price = 1299.99m,
                StockQuantity = 50,
                CategoryId = 1,
                IsAvailable = true
            };

            var createdProduct = await productService.CreateAsync(newProduct);
            Console.WriteLine($"Created product: ID={createdProduct.Id}, Name={createdProduct.Name}, Price=${createdProduct.Price}");

            // Batch create products
            var products = new[]
            {
                new Product
                {
                    Name = "Wireless Mouse",
                    Description = "Ergonomic wireless mouse",
                    Price = 29.99m,
                    StockQuantity = 100,
                    CategoryId = 2,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Mechanical Keyboard",
                    Description = "RGB mechanical gaming keyboard",
                    Price = 89.99m,
                    StockQuantity = 30,
                    CategoryId = 2,
                    IsAvailable = true
                }
            };

            var createdProducts = await productService.CreateBatchAsync(products);
            Console.WriteLine($"Created {createdProducts.Count()} products in batch");

            // Update product
            var productToUpdate = await productService.GetByIdAsync(createdProduct.Id);
            if (productToUpdate != null)
            {
                productToUpdate.Price = 1199.99m;
                productToUpdate.StockQuantity = 45;
                await productService.UpdateAsync(productToUpdate);
                Console.WriteLine($"Updated product: New price=${productToUpdate.Price}, Stock={productToUpdate.StockQuantity}");
            }

            // Conditional query
            var expensiveProducts = await productService.GetByConditionAsync(
                "price > @MinPrice AND is_available = @IsAvailable",
                new { MinPrice = 50.00m, IsAvailable = true });
            Console.WriteLine($"Expensive products (> $50): {expensiveProducts.Count()}");

            // Delete product
            var deleted = await productService.DeleteAsync(createdProduct.Id);
            Console.WriteLine($"Product deleted: {deleted}");

            // Batch delete
            var productIdsToDelete = new[] { createdProducts.First().Id, createdProducts.Last().Id };
            var deletedCount = await productService.DeleteBatchAsync(productIdsToDelete);
            Console.WriteLine($"Deleted {deletedCount} products in batch");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in product CRUD example: {ex.Message}");
        }
        finally
        {
            productService.Dispose();
        }
    }

    /// <summary>
    /// Category CRUD operations example
    /// </summary>
    public async Task CategoryCrudExample()
    {
        Console.WriteLine("\n=== Category CRUD Example ===");

        var categoryService = new CategoryService(_connectionString);

        try
        {
            // Create root category
            var electronicsCategory = new Category
            {
                Name = "Electronics",
                Description = "Electronic devices and accessories",
                IsEnabled = true,
                SortOrder = 1
            };

            var createdCategory = await categoryService.CreateAsync(electronicsCategory);
            Console.WriteLine($"Created category: ID={createdCategory.Id}, Name={createdCategory.Name}");

            // Create sub-category
            var computerCategory = new Category
            {
                Name = "Computers",
                Description = "Computer hardware and software",
                ParentId = createdCategory.Id,
                IsEnabled = true,
                SortOrder = 1
            };

            var createdSubCategory = await categoryService.CreateAsync(computerCategory);
            Console.WriteLine($"Created sub-category: ID={createdSubCategory.Id}, Name={createdSubCategory.Name}, ParentId={createdSubCategory.ParentId}");

            // Get all categories
            var allCategories = await categoryService.GetAllAsync();
            Console.WriteLine($"Total categories: {allCategories.Count()}");

            // Query by parent category
            var childCategories = await categoryService.GetByConditionAsync(
                "parent_id = @ParentId AND is_enabled = @IsEnabled",
                new { ParentId = createdCategory.Id, IsEnabled = true });
            Console.WriteLine($"Child categories: {childCategories.Count()}");

            // Update category
            createdCategory.Description = "Updated electronics category description";
            createdCategory.SortOrder = 2;
            await categoryService.UpdateAsync(createdCategory);
            Console.WriteLine($"Updated category: Description='{createdCategory.Description}', SortOrder={createdCategory.SortOrder}");

            // Delete category
            var deleted = await categoryService.DeleteAsync(createdSubCategory.Id);
            Console.WriteLine($"Sub-category deleted: {deleted}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in category CRUD example: {ex.Message}");
        }
        finally
        {
            categoryService.Dispose();
        }
    }

    /// <summary>
    /// Transaction operations example
    /// </summary>
    public async Task TransactionExample()
    {
        Console.WriteLine("\n=== Transaction Example ===");

        var userService = new UserService(_connectionString);
        var productService = new ProductService(_connectionString);

        try
        {
            // Use transaction helper for transaction operations
            using var transactionHelper = new TransactionHelper(_connectionString);

            await transactionHelper.ExecuteInTransactionAsync(async () =>
            {
                // Create user in transaction
                var user = new User
                {
                    Username = "transaction_user",
                    Email = "transaction@example.com",
                    PasswordHash = "transaction_hash",
                    IsActive = true
                };
                await userService.CreateAsync(user);

                // Create product in transaction
                var product = new Product
                {
                    Name = "Transaction Product",
                    Description = "Product created in transaction",
                    Price = 99.99m,
                    StockQuantity = 10,
                    CategoryId = 1,
                    IsAvailable = true
                };
                await productService.CreateAsync(product);

                Console.WriteLine("Transaction completed successfully!");
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Transaction failed: {ex.Message}");
        }
        finally
        {
            userService.Dispose();
            productService.Dispose();
        }
    }

    /// <summary>
    /// Query builder example
    /// </summary>
    public void QueryBuilderExample()
    {
        Console.WriteLine("\n=== Query Builder Example ===");

        // Basic query
        var basicQuery = new QueryBuilder()
            .Select("id", "username", "email")
            .From("users")
            .Where("is_active = @IsActive", true)
            .OrderBy("created_at")
            .Limit(10)
            .Build();

        Console.WriteLine($"Basic query: {basicQuery}");

        // Complex query
        var complexQuery = new QueryBuilder()
            .Select("p.id", "p.name", "p.price", "c.name as category_name")
            .From("products", "p")
            .InnerJoin("categories", "c.id = p.category_id", "c")
            .Where("p.is_available = @IsAvailable", true)
            .AndWhere("p.price BETWEEN @MinPrice AND @MaxPrice", 50.00m, 500.00m)
            .OrderByDesc("p.created_at")
            .Limit(20, 0)
            .Build();

        Console.WriteLine($"Complex query: {complexQuery}");

        // Aggregate query
        var aggregateQuery = new QueryBuilder()
            .Select("category_id", "COUNT(*) as product_count", "AVG(price) as avg_price")
            .From("products")
            .Where("is_available = @IsAvailable", true)
            .GroupBy("category_id")
            .Having("COUNT(*) > @MinCount", 5)
            .Build();

        Console.WriteLine($"Aggregate query: {aggregateQuery}");
    }

    /// <summary>
    /// Run all examples
    /// </summary>
    public async Task RunAllExamples()
    {
        await UserCrudExample();
        await ProductCrudExample();
        await CategoryCrudExample();
        await TransactionExample();
        QueryBuilderExample();
    }
}