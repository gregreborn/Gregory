namespace TP2_API.DTOs;

public class UserDto
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? PostgresUsername { get; set; } // PostgreSQL username
    public string? PostgresPassword { get; set; } // PostgreSQL password
    public bool IsAdmin { get; set; } // Indicates if the user is an admin
}
