using System;
using System.Linq;
using System.Security.Cryptography;

namespace TP2_API.Utils
{
    public static class PasswordGenerator
    {
        private const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        private const int PasswordLength = 12; 

        public static string GenerateSecurePassword()
        {
            // Create a buffer to hold the random bytes
            var byteBuffer = new byte[PasswordLength];

            // Fill the buffer with random bytes
            RandomNumberGenerator.Fill(byteBuffer);

            // Convert each byte to a character from the AllowedChars string
            var passwordChars = byteBuffer.Select(b => AllowedChars[b % AllowedChars.Length]).ToArray();

            // Convert the character array to a string and return it
            return new string(passwordChars);
        }
    }
}