namespace ClientApplication.Database.Models;

/// <summary>
/// User entity model representing user data structure
/// </summary>
public class User
{
    /// <summary>
    /// User unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User's username (must be unique)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User's password hash (securely stored)
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Parameterless constructor for ORM and serialization
    /// </summary>
    public User()
    {
    }

    /// <summary>
    /// Constructor with required parameters
    /// </summary>
    /// <param name="username">User's username</param>
    /// <param name="passwordHash">User's password hash</param>
    public User(string username, string passwordHash)
    {
        Username = username;
        PasswordHash = passwordHash;
    }

    /// <summary>
    /// Constructor with all parameters
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="username">User's username</param>
    /// <param name="passwordHash">User's password hash</param>
    public User(int id, string username, string passwordHash)
    {
        Id = id;
        Username = username;
        PasswordHash = passwordHash;
    }

    /// <summary>
    /// Updates the user's password hash
    /// </summary>
    /// <param name="newPasswordHash">New password hash</param>
    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    /// <summary>
    /// Returns string representation of the user
    /// </summary>
    /// <returns>User information string</returns>
    public override string ToString()
    {
        return $"User[ID={Id}, Username={Username}]";
    }
}