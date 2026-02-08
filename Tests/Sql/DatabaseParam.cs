namespace Tests.Sql;

/// <summary>
/// Database configuration parameters for testing
/// Contains admin credentials for database management
/// </summary>
public static class DatabaseParam
{
    public const string AdminServer = "localhost";
    public const string AdminUid = "root";
    public const string AdminPwd = "123456";

    /// <summary>
    /// Gets the admin connection string (without database specification)
    /// Used for creating/dropping test databases
    /// </summary>
    public static string AdminConnectionString =>
        $"Server={AdminServer};Uid={AdminUid};Pwd={AdminPwd};";
}