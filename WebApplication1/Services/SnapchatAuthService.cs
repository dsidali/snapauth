using System.Security.Cryptography;
using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class SnapchatAuthService : ISnapchatAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenStorage _tokenStorage;
        private readonly IConfiguration _configuration;
        private readonly SnapchatConfig _config;

        public SnapchatAuthService(
            IHttpClientFactory httpClientFactory,
            ITokenStorage tokenStorage,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _tokenStorage = tokenStorage;
            _configuration = configuration;

            _config = new SnapchatConfig
            {
                ClientId = _configuration["Snapchat:ClientId"] ?? "",
                ClientSecret = _configuration["Snapchat:ClientSecret"] ?? "",
                RedirectUri = _configuration["Snapchat:RedirectUri"] ?? "https://localhost:7000/api/auth/callback",
                Scope = _configuration["Snapchat:Scope"] ?? "snapchat-marketing-api"
            };
        }

        public string GenerateAuthorizationUrl(string state = null)
        {
            if (string.IsNullOrEmpty(state))
            {
                state = GenerateRandomState();
            }

            var authUrl = "https://accounts.snapchat.com/login/oauth2/authorize";
            var queryParams = new Dictionary<string, string>
            {
                ["response_type"] = "code",
                ["client_id"] = _config.ClientId,
                ["redirect_uri"] = _config.RedirectUri,
                ["scope"] = _config.Scope,
                ["state"] = state
            };

          // var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={(kvp.Value)}"));
            return $"{authUrl}?{queryString}";
        }

        public async Task<SnapchatTokenResponse> ExchangeCodeForTokenAsync(string code)
        {
            var client = _httpClientFactory.CreateClient();
            var tokenEndpoint = "https://accounts.snapchat.com/login/oauth2/access_token";

            var formData = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("client_id", _config.ClientId),
            new KeyValuePair<string, string>("client_secret", _config.ClientSecret),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("redirect_uri", _config.RedirectUri)
        });

            var response = await client.PostAsync(tokenEndpoint, formData);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SnapchatTokenResponse>(json);
        }

        public async Task<SnapchatTokenResponse> RefreshAccessTokenAsync(string refreshToken)
        {
            var client = _httpClientFactory.CreateClient();
            var tokenEndpoint = "https://accounts.snapchat.com/login/oauth2/access_token";

            var formData = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("refresh_token", refreshToken),
            new KeyValuePair<string, string>("client_id", _config.ClientId),
            new KeyValuePair<string, string>("client_secret", _config.ClientSecret),
            new KeyValuePair<string, string>("grant_type", "refresh_token")
        });

            var response = await client.PostAsync(tokenEndpoint, formData);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SnapchatTokenResponse>(json);
        }

        public async Task<StoredToken> GetValidTokenAsync(string userId)
        {
            var storedToken = await _tokenStorage.GetTokenAsync(userId);

            if (storedToken == null)
            {
                return null;
            }

            // Check if token is expired or about to expire (5 minutes buffer)
            if (storedToken.ExpiresAt <= DateTime.UtcNow.AddMinutes(5))
            {
                try
                {
                    var newTokenResponse = await RefreshAccessTokenAsync(storedToken.RefreshToken);

                    storedToken = new StoredToken
                    {
                        AccessToken = newTokenResponse.access_token,
                        RefreshToken = newTokenResponse.refresh_token ?? storedToken.RefreshToken,
                        ExpiresAt = DateTime.UtcNow.AddSeconds(newTokenResponse.expires_in),
                        UserId = userId
                    };

                    await _tokenStorage.StoreTokenAsync(userId, storedToken);
                }
                catch
                {
                    // If refresh fails, remove the invalid token
                    await _tokenStorage.RemoveTokenAsync(userId);
                    return null;
                }
            }

            return storedToken;
        }

        private string GenerateRandomState()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
    }
}
