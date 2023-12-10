

using System.ComponentModel.DataAnnotations.Schema;

namespace TP2_API.Models;

public class Users
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
    [Column("user_id")]
    public int? UserId { get; set; }
    [Column("username")]
    public string? Username { get; set; }
    [Column("password_hash")]
    public string? PasswordHash { get; set; }
    [Column("is_admin")]
    public bool IsAdmin { get; set; }

    [Column("last_login_date")] public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;
    [Column("postgres_username")]
    public string? PostgresUsername { get; set; }
    [Column("postgres_password")]
    public string? PostgresPassword { get; set; }
}