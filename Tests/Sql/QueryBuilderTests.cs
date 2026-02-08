using Sql.Helpers;

namespace Tests.Sql;

public class QueryBuilderTests
{
    [Fact]
    public void Constructor_ShouldCreateEmptyQueryBuilder()
    {
        // Act
        var queryBuilder = new QueryBuilder();

        // Assert
        queryBuilder.Should().NotBeNull();
    }

    [Fact]
    public void Select_WithSingleColumn_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.Select(["id"]).Build(); // Be explicit about the array

        // Assert
        result.Should().Be("SELECT id");
    }

    [Fact]
    public void Select_WithMultipleColumns_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.Select(["id", "name", "email"]).Build(); // Be explicit about the array

        // Assert
        result.Should().Be("SELECT id, name, email");
    }

    [Fact]
    public void Select_WithNullColumns_ShouldThrowArgumentException()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var act = () => queryBuilder.Select((string[])null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Columns cannot be null or empty*");
    }

    [Fact]
    public void Select_WithEmptyColumns_ShouldThrowArgumentException()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var act = () => queryBuilder.Select(Array.Empty<string>());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Columns cannot be null or empty*");
    }

    [Fact]
    public void Select_WithTableAliasAndColumns_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.Select("u", "id", "name").Build();

        // Assert
        result.Should().Be("SELECT u.id, u.name");
    }

    [Fact]
    public void Select_WithEmptyTableAlias_ShouldThrowArgumentException()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var act = () => queryBuilder.Select("", "id");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Table alias cannot be null or empty*");
    }

    [Fact]
    public void Select_WithWhitespaceTableAlias_ShouldThrowArgumentException()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var act = () => queryBuilder.Select("   ", "id");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Table alias cannot be null or empty*");
    }

    [Fact]
    public void Distinct_WithoutSelect_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.Distinct().From("users").Build();

        // Assert
        result.Should().Be("SELECT * FROM users");
    }

    [Fact]
    public void Distinct_WithSelect_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.Distinct().Select(["name"]).From("users").Build(); // Be explicit about the array

        // Assert
        result.Should().Be("SELECT DISTINCT name FROM users");
    }

    [Fact]
    public void From_WithTableNameOnly_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users").Build();

        // Assert
        result.Should().Be("SELECT * FROM users");
    }

    [Fact]
    public void From_WithTableNameAndAlias_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users", "u").Build();

        // Assert
        result.Should().Be("SELECT * FROM users AS u");
    }

    [Fact]
    public void From_WithEmptyTableName_ShouldThrowArgumentException()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var act = () => queryBuilder.From("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Table name cannot be null or empty*");
    }

    [Fact]
    public void From_WithNullTableName_ShouldThrowArgumentException()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var act = () => queryBuilder.From(null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Table name cannot be null or empty*");
    }

    [Fact]
    public void InnerJoin_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .InnerJoin("orders", "users.id = orders.user_id")
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users INNER JOIN orders ON users.id = orders.user_id");
    }

    [Fact]
    public void InnerJoin_WithAlias_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users", "u")
            .InnerJoin("orders", "u.id = o.user_id", "o")
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users AS u INNER JOIN orders AS o ON u.id = o.user_id");
    }

    [Fact]
    public void LeftJoin_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .LeftJoin("orders", "users.id = orders.user_id")
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users LEFT JOIN orders ON users.id = orders.user_id");
    }

    [Fact]
    public void RightJoin_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .RightJoin("orders", "users.id = orders.user_id")
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users RIGHT JOIN orders ON users.id = orders.user_id");
    }

    [Fact]
    public void Join_WithEmptyTable_ShouldThrowArgumentException()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var act = () => queryBuilder.InnerJoin("", "condition");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Table name cannot be null or empty*");
    }

    [Fact]
    public void Join_WithEmptyCondition_ShouldThrowArgumentException()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var act = () => queryBuilder.InnerJoin("table", "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Join condition cannot be null or empty*");
    }

    [Fact]
    public void Where_WithSimpleCondition_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .Where("age > 18")
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users WHERE age > 18");
    }

    [Fact]
    public void Where_WithParameters_ShouldBuildCorrectQueryAndStoreParameters()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .Where("name = ? AND age > ?", "John", 25)
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users WHERE name = ? AND age > ?");
        queryBuilder.GetParameters().Should().ContainInOrder("John", "25");
    }

    [Fact]
    public void Where_WithEmptyCondition_ShouldThrowArgumentException()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var act = () => queryBuilder.Where("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Condition cannot be null or empty*");
    }

    [Fact]
    public void AndWhere_ShouldAddAndCondition()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .Where("age > 18")
            .AndWhere("status = 'active'")
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users WHERE age > 18 AND status = 'active'");
    }

    [Fact]
    public void AndWhere_WithParameters_ShouldAddAndConditionAndParameters()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .Where("age > ?", 18)
            .AndWhere("name = ?", "John")
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users WHERE age > ? AND name = ?");
        queryBuilder.GetParameters().Should().ContainInOrder("18", "John");
    }

    [Fact]
    public void OrWhere_ShouldAddOrCondition()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .Where("age < 18")
            .OrWhere("age > 65")
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users WHERE age < 18 OR age > 65");
    }

    [Fact]
    public void OrWhere_WithParameters_ShouldAddOrConditionAndParameters()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .Where("age < ?", 18)
            .OrWhere("age > ?", 65)
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users WHERE age < ? OR age > ?");
        queryBuilder.GetParameters().Should().ContainInOrder("18", "65");
    }

    [Fact]
    public void OrderBy_WithSingleColumn_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .OrderBy("name")
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users ORDER BY name");
    }

    [Fact]
    public void OrderBy_WithMultipleColumns_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .OrderBy("name", "age")
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users ORDER BY name, age");
    }

    [Fact]
    public void OrderBy_WithEmptyColumns_ShouldThrowArgumentException()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var act = () => queryBuilder.OrderBy(Array.Empty<string>());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Columns cannot be null or empty*");
    }

    [Fact]
    public void OrderByDesc_WithSingleColumn_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .OrderByDesc("created_at")
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users ORDER BY created_at DESC");
    }

    [Fact]
    public void OrderByDesc_WithMultipleColumns_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .OrderByDesc("created_at", "id")
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users ORDER BY created_at DESC, id DESC");
    }

    [Fact]
    public void GroupBy_WithSingleColumn_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("orders")
            .Select(["user_id", "COUNT(*) as order_count"]) // Be explicit about the array
            .GroupBy("user_id")
            .Build();

        // Assert
        result.Should().Be("SELECT user_id, COUNT(*) as order_count FROM orders GROUP BY user_id");
    }

    [Fact]
    public void GroupBy_WithMultipleColumns_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("orders")
            .Select(["user_id", "status", "COUNT(*) as count"]) // Be explicit about the array
            .GroupBy("user_id", "status")
            .Build();

        // Assert
        result.Should().Be("SELECT user_id, status, COUNT(*) as count FROM orders GROUP BY user_id, status");
    }

    [Fact]
    public void Having_WithCondition_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("orders")
            .Select(["user_id", "COUNT(*) as order_count"]) // Be explicit about the array
            .GroupBy("user_id")
            .Having("COUNT(*) > 5")
            .Build();

        // Assert
        result.Should().Be("SELECT user_id, COUNT(*) as order_count FROM orders GROUP BY user_id HAVING COUNT(*) > 5");
    }

    [Fact]
    public void Having_WithParameters_ShouldBuildCorrectQueryAndStoreParameters()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("orders")
            .Select(["user_id", "COUNT(*) as order_count"]) // Be explicit about the array
            .GroupBy("user_id")
            .Having("COUNT(*) > ?", 5)
            .Build();

        // Assert
        result.Should().Be("SELECT user_id, COUNT(*) as order_count FROM orders GROUP BY user_id HAVING COUNT(*) > ?");
        queryBuilder.GetParameters().Should().Contain("5");
    }

    [Fact]
    public void Limit_WithPositiveValue_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .Limit(10)
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users LIMIT 10");
    }

    [Fact]
    public void Limit_WithZeroValue_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .Limit(0)
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users LIMIT 0");
    }

    [Fact]
    public void Limit_WithNegativeValue_ShouldThrowArgumentException()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var act = () => queryBuilder.Limit(-1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Limit must be non-negative*");
    }

    [Fact]
    public void Offset_WithPositiveValue_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .Limit(10)
            .Offset(20)
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users LIMIT 10 OFFSET 20");
    }

    [Fact]
    public void Offset_WithZeroValue_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .Limit(10)
            .Offset(0)
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users LIMIT 10 OFFSET 0");
    }

    [Fact]
    public void Offset_WithNegativeValue_ShouldThrowArgumentException()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var act = () => queryBuilder.Offset(-1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Offset must be non-negative*");
    }

    [Fact]
    public void Limit_WithLimitAndOffset_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .Limit(10, 20)
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users LIMIT 10 OFFSET 20");
    }

    [Fact]
    public void ComplexQuery_ShouldBuildCompleteQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder
            .Distinct()
            .Select("u", "id", "name", "email")
            .From("users", "u")
            .InnerJoin("profiles", "u.id = p.user_id", "p")
            .Where("u.active = ?", true)
            .AndWhere("p.age > ?", 18)
            .OrderBy("u.created_at")
            .Limit(50, 100)
            .Build();

        // Assert
        result.Should().Be("SELECT DISTINCT u.id, u.name, u.email FROM users AS u INNER JOIN profiles AS p ON u.id = p.user_id WHERE u.active = ? AND p.age > ? ORDER BY u.created_at LIMIT 50 OFFSET 100");
        queryBuilder.GetParameters().Should().ContainInOrder("True", "18");
    }

    [Fact]
    public void GetParameters_WhenNoParameters_ShouldReturnEmptyList()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var parameters = queryBuilder.GetParameters();

        // Assert
        parameters.Should().BeEmpty();
    }

    [Fact]
    public void GetParameters_WhenHasParameters_ShouldReturnParameters()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        queryBuilder.Where("name = ? AND age > ?", "John", 25);
        var parameters = queryBuilder.GetParameters();

        // Assert
        parameters.Should().ContainInOrder("John", "25");
    }

    [Fact]
    public void GetParameters_ReturnedListShouldBeReadOnly()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();
        queryBuilder.Where("name = ?", "John");

        // Act
        var parameters = queryBuilder.GetParameters();

        // Assert
        parameters.Should().BeAssignableTo<IReadOnlyList<string>>();
    }

    [Fact]
    public void Clear_ShouldResetAllClauses()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();
        queryBuilder.Select(["id"]) // Be explicit about the array
            .From("users")
            .Where("active = ?", true)
            .OrderBy("created_at")
            .Limit(10);

        // Act
        queryBuilder.Clear();
        var result = queryBuilder.Build();

        // Assert
        result.Should().Be("SELECT *");
        queryBuilder.GetParameters().Should().BeEmpty();
    }

    [Fact]
    public void Clear_ShouldResetAllProperties()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();
        queryBuilder.Distinct().Limit(10).Offset(5);

        // Act
        queryBuilder.Clear();

        // Assert
        var result = queryBuilder.Build();
        result.Should().Be("SELECT *");
        queryBuilder.GetParameters().Should().BeEmpty();
    }

    [Fact]
    public void Clone_ShouldCreateIndependentCopy()
    {
        // Arrange
        var original = new QueryBuilder();
        original.Select(["id", "name"]) // Be explicit about the array
            .From("users")
            .Where("active = ?", true);

        // Act
        var clone = original.Clone();
        clone.AndWhere("age > ?", 18); // Modify clone
        var originalResult = original.Build();
        var cloneResult = clone.Build();

        // Assert
        originalResult.Should().Be("SELECT id, name FROM users WHERE active = ?");
        cloneResult.Should().Be("SELECT id, name FROM users WHERE active = ? AND age > ?");
        original.GetParameters().Should().ContainInOrder("True");
        clone.GetParameters().Should().ContainInOrder("True", "18");
    }

    [Fact]
    public void Clone_ShouldCopyAllProperties()
    {
        // Arrange
        var original = new QueryBuilder();
        original.Distinct()
            .Select(["count(*)"]) // Be explicit about the array
            .From("users")
            .Limit(5);

        // Act
        var clone = original.Clone();

        // Assert
        clone.Build().Should().Be(original.Build());
        clone.GetParameters().Should().Equal(original.GetParameters());
    }

    [Fact]
    public void FluentInterface_ShouldAllowMethodChaining()
    {
        // Arrange & Act
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder
            .Select(["id", "name"]) // Be explicit about the array
            .From("users")
            .Where("age > ?", 18)
            .AndWhere("active = ?", true)
            .OrderBy("name")
            .Limit(10)
            .Build();

        // Assert
        result.Should().Be("SELECT id, name FROM users WHERE age > ? AND active = ? ORDER BY name LIMIT 10");
    }

    [Fact]
    public void Build_WithoutAnyClauses_ShouldReturnDefaultSelect()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.Build();

        // Assert
        result.Should().Be("SELECT *");
    }

    [Fact]
    public void Build_WithOnlySelect_ShouldReturnSelectClause()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.Select(["id", "name"]).Build();

        // Assert
        result.Should().Be("SELECT id, name");
    }

    [Fact]
    public void Build_WithNullParameter_ShouldThrowException()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var act = () => queryBuilder.From("users")
            .Where("name = ?", null!)
            .Build();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MultipleWhereConditions_ShouldChainCorrectly()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("products")
            .Where("category = ?", "Electronics")
            .AndWhere("price BETWEEN ? AND ?", 100, 500)
            .AndWhere("in_stock = ?", true)
            .OrWhere("featured = ?", true)
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM products WHERE category = ? AND price BETWEEN ? AND ? AND in_stock = ? OR featured = ?");
        queryBuilder.GetParameters().Should().ContainInOrder("Electronics", "100", "500", "True", "True");
    }
}