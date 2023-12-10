using System;
using System.Security.Cryptography;
using System.Text;

namespace TP2_API.Utils;

public class PasswordHasher
{
    // Global salt (keep it secret)
    private const string GlobalSalt = "YourSecretGlobalSaltHere";

    // Hash the password using the global salt
    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = Encoding.UTF8.GetBytes(GlobalSalt + password);
            var hashedPasswordBytes = sha256.ComputeHash(saltedPassword);
            return Convert.ToBase64String(hashedPasswordBytes);
        }
    }

    // Verify a password by comparing it with the stored hash
    public static bool VerifyPassword(string password, string storedHash)
    {
        var hashedPassword = HashPassword(password);
        return string.Equals(hashedPassword, storedHash);
    }
}