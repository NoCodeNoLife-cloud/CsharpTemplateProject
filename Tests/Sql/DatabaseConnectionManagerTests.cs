using MySqlConnector;
using Sql.Helpers;

namespace Tests.Sql;

public class DatabaseConnectionManagerTests : IDisposable
{
    private readonly DatabaseConnectionManager _connectionManager;
    private bool _databaseCreated;

    public DatabaseConnectionManagerTests()
    {
        SetupTestDatabase();
        _connectionManager = new DatabaseConnectionManager(DatabaseParam.TestConnectionString);
    }

    private void SetupTestDatabase()
    {
        try
        {
            // Connect to MySQL server without specifying database
            using var connection = new MySqlConnection(DatabaseParam.AdminConnectionString);
            connection.Open();

            // Check if test database exists
            using var cmd = new MySqlCommand($"SHOW DATABASES LIKE '{DatabaseParam.TestDatabaseName}'", connection);
            var result = cmd.ExecuteScalar();

            if (result != null) return;
            // Create test database
            using var createCmd = new MySqlCommand($"CREATE DATABASE {DatabaseParam.TestDatabaseName}", connection);
            createCmd.ExecuteNonQuery();
            _databaseCreated = true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to setup test database: {ex.Message}", ex);
        }
    }

    private void CleanupTestDatabase()
    {
        if (!_databaseCreated) return;

        try
        {
            // Connect to MySQL server without specifying database
            using var connection = new MySqlConnection(DatabaseParam.AdminConnectionString);
            connection.Open();

            // Drop test database
            using var cmd = new MySqlCommand($"DROP DATABASE IF EXISTS {DatabaseParam.TestDatabaseName}", connection);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            // Log the error but don't throw in cleanup
            Console.WriteLine($"Warning: Failed to cleanup test database: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _connectionManager.Dispose();
        CleanupTestDatabase();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_WithValidConnectionString_ShouldInitializeSuccessfully()
    {
        // Act
        var manager = new DatabaseConnectionManager(DatabaseParam.TestConnectionString);

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
        var result = await _connectionManager.GetConnectionAsync();

        // Assert
        result.Should().NotBeNull();
        result.ConnectionString.Should().Contain($"Database={DatabaseParam.TestDatabaseName}");
        result.ConnectionString.Should().Contain($"Server={DatabaseParam.AdminServer}");
    }

    [Fact]
    public void GetConnection_WithValidConnectionString_ShouldReturnConnection()
    {
        // Act
        var result = _connectionManager.GetConnection();

        // Assert
        result.Should().NotBeNull();
        result.ConnectionString.Should().Contain($"Database={DatabaseParam.TestDatabaseName}");
        result.ConnectionString.Should().Contain($"Server={DatabaseParam.AdminServer}");
    }

    [Fact]
    public void IsConnectionAvailable_WithValidConnectionString_ShouldReturnTrue()
    {
        // Act
        var result = _connectionManager.IsConnectionAvailable();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsConnectionAvailableAsync_WithValidConnectionString_ShouldReturnTrue()
    {
        // Act
        var result = await _connectionManager.IsConnectionAvailableAsync();

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
        var act = () => _connectionManager.CloseConnection();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task CloseConnectionAsync_WhenCalled_ShouldNotThrowException()
    {
        // Act
        var act = async () => await _connectionManager.CloseConnectionAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Dispose_WhenCalled_ShouldNotThrowException()
    {
        // Arrange
        var manager = new DatabaseConnectionManager(DatabaseParam.TestConnectionString);

        // Act
        var act = () => manager.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_ShouldNotThrowException()
    {
        // Arrange
        var manager = new DatabaseConnectionManager(DatabaseParam.TestConnectionString);

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
        var connection1 = await _connectionManager.GetConnectionAsync();
        var connection2 = await _connectionManager.GetConnectionAsync();

        // Assert
        connection1.Should().BeSameAs(connection2);
    }

    [Fact]
    public void MultipleCalls_GetConnection_ShouldReturnSameConnection()
    {
        // Act
        var connection1 = _connectionManager.GetConnection();
        var connection2 = _connectionManager.GetConnection();

        // Assert
        connection1.Should().BeSameAs(connection2);
    }

    [Fact]
    public async Task Connection_StateAfterGetConnectionAsync_ShouldBeOpen()
    {
        // Act
        var connection = await _connectionManager.GetConnectionAsync();

        // Assert
        connection.State.Should().Be(System.Data.ConnectionState.Open);
    }

    [Fact]
    public void Connection_StateAfterGetConnection_ShouldBeOpen()
    {
        // Act
        var connection = _connectionManager.GetConnection();

        // Assert
        connection.State.Should().Be(System.Data.ConnectionState.Open);
    }

    [Fact]
    public async Task GetConnectionAsync_AfterCloseConnection_ShouldReopenConnection()
    {
        // Arrange
        await _connectionManager.GetConnectionAsync();

        // Act
        await _connectionManager.CloseConnectionAsync();
        var newConnection = await _connectionManager.GetConnectionAsync();

        // Assert
        newConnection.Should().NotBeNull();
        newConnection.State.Should().Be(System.Data.ConnectionState.Open);
    }

    [Fact]
    public void GetConnection_AfterCloseConnection_ShouldReopenConnection()
    {
        // Arrange
        _connectionManager.GetConnection();

        // Act
        _connectionManager.CloseConnection();
        var newConnection = _connectionManager.GetConnection();

        // Assert
        newConnection.Should().NotBeNull();
        newConnection.State.Should().Be(System.Data.ConnectionState.Open);
    }
}