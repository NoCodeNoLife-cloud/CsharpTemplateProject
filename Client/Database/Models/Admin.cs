namespace Client.Database.Models;

/// <summary>
/// Admin entity model representing administrator data structure
/// </summary>
public class Admin
{
    /// <summary>
    /// Admin unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Admin's username (must be unique)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Admin's password hash (securely stored)
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Admin role level (super/senior/junior)
    /// </summary>
    public string Role { get; set; } = "junior";

    /// <summary>
    /// Admin department or division
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Admin contact email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Admin creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLogin { get; set; }

    /// <summary>
    /// Whether the admin account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Parameterless constructor for ORM and serialization
    /// </summary>
    public Admin()
    {
    }

    /// <summary>
    /// Constructor with required parameters
    /// </summary>
    /// <param name="username">Admin's username</param>
    /// <param name="passwordHash">Admin's password hash</param>
    /// <param name="role">Admin role level (default: junior)</param>
    public Admin(string username, string passwordHash, string role = "junior")
    {
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Constructor with all parameters
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <param name="username">Admin's username</param>
    /// <param name="passwordHash">Admin's password hash</param>
    /// <param name="role">Admin role level</param>
    /// <param name="department">Admin department</param>
    /// <param name="email">Admin email</param>
    public Admin(int id, string username, string passwordHash, string role = "junior", 
                 string department = "", string email = "")
    {
        Id = id;
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
        Department = department;
        Email = email;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the admin's password hash
    /// </summary>
    /// <param name="newPasswordHash">New password hash</param>
    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    /// <summary>
    /// Updates last login timestamp
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLogin = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the admin account
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Activates the admin account
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Checks if admin has super role
    /// </summary>
    /// <returns>True if super admin</returns>
    public bool IsSuperAdmin()
    {
        return Role.Equals("super", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if admin has senior role
    /// </summary>
    /// <returns>True if senior admin</returns>
    public bool IsSeniorAdmin()
    {
        return Role.Equals("senior", StringComparison.OrdinalIgnoreCase) || 
               Role.Equals("super", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns string representation of the admin
    /// </summary>
    /// <returns>Admin information string</returns>
    public override string ToString()
    {
        return $"Admin[ID={Id}, Username={Username}, Role={Role}, Department={Department}, IsActive={IsActive}]";
    }
}