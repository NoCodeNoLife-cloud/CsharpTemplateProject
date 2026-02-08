namespace Tests.Sql;

public static class DatabaseParam
{
    public const string AdminServer = "localhost";
    public const string AdminUid = "root";
    public const string AdminPwd = "123456";
    public const string TestDatabaseName = "testdb";

    public static string AdminConnectionString =>
        $"Server={AdminServer};Uid={AdminUid};Pwd={AdminPwd};";

    public static string TestConnectionString =>
        $"Server={AdminServer};Database={TestDatabaseName};Uid={AdminUid};Pwd={AdminPwd};";
}