using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

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

    public static List<List<string>> HashListToHex(List<string> inputs)
    {
        if (inputs == null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        var hashedValues = new List<List<string>>(inputs.Count);

        foreach (var input in inputs)
        {
            var hashedValues1 = new List<string>();

            hashedValues1.Add(HashToHex(input).ToLower());
            hashedValues.Add(hashedValues1);
        }

        return hashedValues;
    }
}
