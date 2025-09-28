using System;
using System.Collections.Generic;

namespace WebApplication1.Services;

public class EmailGeneratorService
{
    private static readonly Random _random = new Random();
    private static readonly string[] _domains = { "gmail.com", "yahoo.com", "outlook.com", "hotmail.com", "example.com", "test.com" };
    private static readonly string[] _usernames = { "user", "admin", "test", "john", "jane", "mike", "sarah", "david", "emma", "alex" };

    public List<string> GenerateRandomEmails(int count)
    {
        if (count <= 0)
        {
            throw new ArgumentException("Count must be a positive integer", nameof(count));
        }

        var emails = new List<string>(count);

        for (int i = 0; i < count; i++)
        {
            string username = _usernames[_random.Next(_usernames.Length)];
            string domain = _domains[_random.Next(_domains.Length)];
            string randomNumber = _random.Next(1000, 9999).ToString();

            emails.Add($"{username}{randomNumber}@{domain}");
        }

        return emails;
    }
}
