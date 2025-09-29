using StackExchange.Redis;
using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class RedisTokenStorage : ITokenStorage
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly string _prefix = "snapchat_token:";

        public RedisTokenStorage(IConfiguration configuration)
        {
            var connectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _database = _redis.GetDatabase();
        }

        public async Task StoreTokenAsync(string userId, StoredToken token)
        {
            var key = _prefix + userId;
            var value = JsonSerializer.Serialize(token);

            // Calculate time until expiration
            var timeUntilExpiration = token.ExpiresAt - DateTime.UtcNow;

            // Store with expiration time
         //   await _database.StringSetAsync(key, value, timeUntilExpiration);
            await _database.StringSetAsync(key, value, null); //no expiry
        }

        public async Task<StoredToken> GetTokenAsync(string userId)
        {
            var key = _prefix + userId;
            var value = await _database.StringGetAsync(key);

            if (value.IsNullOrEmpty)
            {
                return null;
            }

            return JsonSerializer.Deserialize<StoredToken>(value);
        }

        public async Task RemoveTokenAsync(string userId)
        {
            var key = _prefix + userId;
            await _database.KeyDeleteAsync(key);
        }
    }
}