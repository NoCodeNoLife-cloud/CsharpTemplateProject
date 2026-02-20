using Sql.Helpers;

namespace Tests.IntegrationTests.Sql.Features.Query;

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
    public void CrossJoin_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .CrossJoin("orders")
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users CROSS JOIN orders");
    }

    [Fact]
    public void Where_WithSingleCondition_ShouldBuildCorrectQuery()
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
    public void Where_WithMultipleConditions_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .Where("age > 18")
            .Where("status = 'active'")
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users WHERE age > 18 AND status = 'active'");
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
            .OrderBy("name", "created_at DESC")
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users ORDER BY name, created_at DESC");
    }

    [Fact]
    public void Limit_WithCountOnly_ShouldBuildCorrectQuery()
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
    public void Limit_WithOffsetAndCount_ShouldBuildCorrectQuery()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.From("users")
            .Limit(10, 20)
            .Build();

        // Assert
        result.Should().Be("SELECT * FROM users LIMIT 20 OFFSET 10");
    }

    [Fact]
    public void ComplexQuery_BuildsCorrectly()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder
            .Select(["u.id", "u.name", "o.total"])
            .From("users", "u")
            .InnerJoin("orders", "o", "u.id = o.user_id")
            .Where("u.status = 'active'")
            .Where("o.total > 100")
            .OrderBy("o.created_at DESC")
            .Limit(0, 50)
            .Build();

        // Assert
        result.Should().Be("SELECT u.id, u.name, o.total FROM users AS u INNER JOIN orders AS o ON u.id = o.user_id WHERE u.status = 'active' AND o.total > 100 ORDER BY o.created_at DESC LIMIT 50 OFFSET 0");
    }

    [Fact]
    public void Clear_ResetsQueryBuilder()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();
        queryBuilder.Select(["id"]).From("users");

        // Act
        queryBuilder.Clear();
        var result = queryBuilder.Build();

        // Assert
        result.Should().Be("");
    }

    [Fact]
    public void Build_ReturnsEmptyString_WhenNoClausesAdded()
    {
        // Arrange
        var queryBuilder = new QueryBuilder();

        // Act
        var result = queryBuilder.Build();

        // Assert
        result.Should().Be("");
    }
}