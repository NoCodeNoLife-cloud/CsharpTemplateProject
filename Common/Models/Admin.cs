using System.ComponentModel.DataAnnotations;

namespace Common.Models;

/// <summary>
/// Admin entity model representing administrator data structure
/// </summary>
public class Admin
{
    /// <summary>
    /// Admin unique identifier
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Admin's username (must be unique)
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Admin's password hash (securely stored)
    /// </summary>
    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Admin role level (super/senior/junior)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Role { get; set; } = "junior";

    /// <summary>
    /// Admin department or division
    /// </summary>
    [StringLength(100)]
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Admin contact email
    /// </summary>
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Admin creation timestamp (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last login timestamp (UTC)
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