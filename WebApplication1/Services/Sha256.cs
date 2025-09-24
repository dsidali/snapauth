using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Services;

public static class Sha256
{
    public static string HashToHex(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = SHA256.HashData(bytes);           // static API in modern .NET
        return Convert.ToHexString(hash);                // uppercase hex; call .ToLowerInvariant() if needed
    }

    public static string HashToBase64(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
