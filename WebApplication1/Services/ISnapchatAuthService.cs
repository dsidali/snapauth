using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface ISnapchatAuthService
    {
        string GenerateAuthorizationUrl(string state = null);
        Task<SnapchatTokenResponse> ExchangeCodeForTokenAsync(string code);
        Task<SnapchatTokenResponse> RefreshAccessTokenAsync(string refreshToken);
        Task<StoredToken> GetValidTokenAsync(string userId);
    }
}
