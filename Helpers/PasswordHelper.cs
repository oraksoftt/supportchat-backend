using System;
using BCrypt.Net;

namespace SupportChat.Backend.Helpers;

public static class PasswordHelper
{
    public static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password must not be null, empty or whitespace.", nameof(password));
        }

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public static bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ArgumentException("Hash must not be null, empty or whitespace.", nameof(hash));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            // Treat missing password as verification failure
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}