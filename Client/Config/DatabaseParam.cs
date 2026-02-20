namespace Client.Config;

/// <summary>
/// Database parameter configuration class
/// </summary>
public static class DatabaseParam
{
    /// <summary>
    /// Administrator connection string
    /// </summary>
    public static string AdminConnectionString => 
        $"Server={AdminServer};Database=mysql;Uid={AdminUid};Pwd={AdminPwd};";

    /// <summary>
    /// Administrator server address
    /// </summary>
    public static string AdminServer { get; set; } = "localhost";

    /// <summary>
    /// Administrator username
    /// </summary>
    public static string AdminUid { get; set; } = "root";

    /// <summary>
    /// Administrator password
    /// </summary>
    public static string AdminPwd { get; set; } = "123456";
}