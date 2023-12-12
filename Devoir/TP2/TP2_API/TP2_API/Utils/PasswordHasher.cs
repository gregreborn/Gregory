using BCrypt.Net;

namespace TP2_API.Utils;

public static class PasswordHasher
{
    // Hash the password using bcrypt
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    // Verify a password by comparing it with the stored hash using bcrypt
    public static bool VerifyPassword(string password, string storedHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, storedHash);
    }
}