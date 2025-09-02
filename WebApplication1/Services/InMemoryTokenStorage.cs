using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class InMemoryTokenStorage : ITokenStorage
    {
        private readonly Dictionary<string, StoredToken> _tokens = new();

        public Task StoreTokenAsync(string userId, StoredToken token)
        {
            _tokens[userId] = token;
            return Task.CompletedTask;
        }

        public Task<StoredToken> GetTokenAsync(string userId)
        {
            _tokens.TryGetValue(userId, out var token);
            return Task.FromResult(token);
        }

        public Task RemoveTokenAsync(string userId)
        {
            _tokens.Remove(userId);
            return Task.CompletedTask;
        }
    }
}
