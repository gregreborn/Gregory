namespace TP2_Interface.Models;

public class LoginResponse
{
    public int UserId { get; set; }
    public string Username { get; set; }

    public bool IsAdmin { get; set; }
    public string PostgresUsername { get; set; }
    public string PostgresPassword { get; set; }
}