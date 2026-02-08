using Sql.Helpers;

namespace Tests.Sql;

public class DatabaseConnectionManagerTests : DatabaseTestBase
{
    [Fact]
    public void Constructor_WithValidConnectionString_ShouldInitializeSuccessfully()
    {
        // Act
        var manager = new DatabaseConnectionManager(TestConnectionString);

        // Assert
        manager.Should().NotBeNull();
        manager.Dispose();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidConnectionString_ShouldThrowArgumentException(string? invalidConnectionString)
    {
        // Act
        var act = () => { _ = new DatabaseConnectionManager(invalidConnectionString!); };

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Connection string cannot be null or empty.*");
    }

    [Fact]
    public async Task GetConnectionAsync_WithValidConnectionString_ShouldReturnConnection()
    {
        // Act
        var result = await ConnectionManager!.GetConnectionAsync();

        // Assert
        result.Should().NotBeNull();
        result.ConnectionString.Should().Contain($"Database={TestDatabaseName}");
        result.ConnectionString.Should().Contain($"Server={DatabaseParam.AdminServer}");
    }

    [Fact]
    public void GetConnection_WithValidConnectionString_ShouldReturnConnection()
    {
        // Act
        var result = ConnectionManager!.GetConnection();

        // Assert
        result.Should().NotBeNull();
        result.ConnectionString.Should().Contain($"Database={TestDatabaseName}");
        result.ConnectionString.Should().Contain($"Server={DatabaseParam.AdminServer}");
    }

    [Fact]
    public void IsConnectionAvailable_WithValidConnectionString_ShouldReturnTrue()
    {
        // Act
        var result = ConnectionManager!.IsConnectionAvailable();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsConnectionAvailableAsync_WithValidConnectionString_ShouldReturnTrue()
    {
        // Act
        var result = await ConnectionManager!.IsConnectionAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsConnectionAvailableAsync_WithInvalidConnectionString_ShouldReturnFalse()
    {
        // Arrange
        const string invalidConnectionString = $"Server={DatabaseParam.AdminServer};Database=nonexistent_db_xyz;Uid={DatabaseParam.AdminUid};Pwd={DatabaseParam.AdminPwd};";
        using var manager = new DatabaseConnectionManager(invalidConnectionString);

        // Act
        var result = await manager.IsConnectionAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsConnectionAvailable_WithInvalidConnectionString_ShouldReturnFalse()
    {
        // Arrange
        const string invalidConnectionString = $"Server={DatabaseParam.AdminServer};Database=nonexistent_db_xyz;Uid={DatabaseParam.AdminUid};Pwd={DatabaseParam.AdminPwd};";
        using var manager = new DatabaseConnectionManager(invalidConnectionString);

        // Act
        var result = manager.IsConnectionAvailable();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CloseConnection_WhenCalled_ShouldNotThrowException()
    {
        // Act
        var act = () => ConnectionManager!.CloseConnection();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task CloseConnectionAsync_WhenCalled_ShouldNotThrowException()
    {
        // Act
        var act = async () => await ConnectionManager!.CloseConnectionAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Dispose_WhenCalled_ShouldNotThrowException()
    {
        // Arrange
        var manager = new DatabaseConnectionManager(TestConnectionString);

        // Act
        var act = () => manager.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_ShouldNotThrowException()
    {
        // Arrange
        var manager = new DatabaseConnectionManager(TestConnectionString);

        // Act
        manager.Dispose();
        var act = () => manager.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task MultipleCalls_GetConnectionAsync_ShouldReturnSameConnection()
    {
        // Act
        var connection1 = await ConnectionManager!.GetConnectionAsync();
        var connection2 = await ConnectionManager.GetConnectionAsync();

        // Assert
        connection1.Should().BeSameAs(connection2);
    }

    [Fact]
    public void MultipleCalls_GetConnection_ShouldReturnSameConnection()
    {
        // Act
        var connection1 = ConnectionManager!.GetConnection();
        var connection2 = ConnectionManager.GetConnection();

        // Assert
        connection1.Should().BeSameAs(connection2);
    }

    [Fact]
    public async Task Connection_StateAfterGetConnectionAsync_ShouldBeOpen()
    {
        // Act
        var connection = await ConnectionManager!.GetConnectionAsync();

        // Assert
        connection.State.Should().Be(System.Data.ConnectionState.Open);
    }

    [Fact]
    public void Connection_StateAfterGetConnection_ShouldBeOpen()
    {
        // Act
        var connection = ConnectionManager!.GetConnection();

        // Assert
        connection.State.Should().Be(System.Data.ConnectionState.Open);
    }

    [Fact]
    public async Task GetConnectionAsync_AfterCloseConnection_ShouldReopenConnection()
    {
        // Arrange
        await ConnectionManager!.GetConnectionAsync();

        // Act
        await ConnectionManager.CloseConnectionAsync();
        var newConnection = await ConnectionManager.GetConnectionAsync();

        // Assert
        newConnection.Should().NotBeNull();
        newConnection.State.Should().Be(System.Data.ConnectionState.Open);
    }

    [Fact]
    public void GetConnection_AfterCloseConnection_ShouldReopenConnection()
    {
        // Arrange
        ConnectionManager!.GetConnection();

        // Act
        ConnectionManager.CloseConnection();
        var newConnection = ConnectionManager.GetConnection();

        // Assert
        newConnection.Should().NotBeNull();
        newConnection.State.Should().Be(System.Data.ConnectionState.Open);
    }
}