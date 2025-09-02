using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface ITokenStorage
    {
        Task StoreTokenAsync(string userId, StoredToken token);
        Task<StoredToken> GetTokenAsync(string userId);
        Task RemoveTokenAsync(string userId);
    }
}
