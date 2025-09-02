using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISnapchatAuthService _authService;
        private readonly ITokenStorage _tokenStorage;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ISnapchatAuthService authService,
            ITokenStorage tokenStorage,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _tokenStorage = tokenStorage;
            _logger = logger;
        }

        /// <summary>
        /// Initiates the Snapchat OAuth2 authorization flow
        /// </summary>
        [HttpGet("authorize")]
        public IActionResult Authorize([FromQuery] string userId = "default")
        {
            try
            {
                var state = $"{userId}:{Guid.NewGuid()}";
                var authUrl = _authService.GenerateAuthorizationUrl(state);

                return Ok(new
                {
                    message = "Open this URL in your browser to authorize",
                    authorizationUrl = authUrl,
                    state = state
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating authorization URL");
                return StatusCode(500, new { error = "Failed to generate authorization URL" });
            }
        }

        /// <summary>
        /// Handles the OAuth2 callback from Snapchat
        /// </summary>
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return BadRequest(new { error = "Authorization code not provided" });
                }

                // Extract userId from state (format: "userId:guid")
                var userId = "default";
                if (!string.IsNullOrEmpty(state))
                {
                    var stateParts = state.Split(':');
                    if (stateParts.Length > 0)
                    {
                        userId = stateParts[0];
                    }
                }

                // Exchange code for tokens
                var tokenResponse = await _authService.ExchangeCodeForTokenAsync(code);

                // Store tokens
                var storedToken = new StoredToken
                {
                    AccessToken = tokenResponse.access_token,
                    RefreshToken = tokenResponse.refresh_token,
                    ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in),
                    UserId = userId
                };

                await _tokenStorage.StoreTokenAsync(userId, storedToken);

                return Ok(new
                {
                    message = "Authorization successful",
                    userId = userId,
                    accessToken = tokenResponse.access_token,
                    expiresIn = tokenResponse.expires_in,
                    tokenType = tokenResponse.token_type
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OAuth callback");
                return StatusCode(500, new { error = "Failed to exchange authorization code" });
            }
        }

        /// <summary>
        /// Gets a valid access token for the user (refreshes if needed)
        /// </summary>
        [HttpGet("token/{userId}")]
        public async Task<IActionResult> GetToken(string userId = "default")
        {
            try
            {
                var token = await _authService.GetValidTokenAsync(userId);

                if (token == null)
                {
                    return NotFound(new { error = "No token found for user. Please authorize first." });
                }

                return Ok(new
                {
                    accessToken = token.AccessToken,
                    expiresAt = token.ExpiresAt,
                    userId = token.UserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token");
                return StatusCode(500, new { error = "Failed to get token" });
            }
        }

        /// <summary>
        /// Manually refreshes the access token
        /// </summary>
        [HttpPost("refresh/{userId}")]
        public async Task<IActionResult> RefreshToken(string userId = "default")
        {
            try
            {
                var storedToken = await _tokenStorage.GetTokenAsync(userId);

                if (storedToken == null)
                {
                    return NotFound(new { error = "No token found for user" });
                }

                var newTokenResponse = await _authService.RefreshAccessTokenAsync(storedToken.RefreshToken);

                // Update stored token
                storedToken.AccessToken = newTokenResponse.access_token;
                storedToken.RefreshToken = newTokenResponse.refresh_token ?? storedToken.RefreshToken;
                storedToken.ExpiresAt = DateTime.UtcNow.AddSeconds(newTokenResponse.expires_in);

                await _tokenStorage.StoreTokenAsync(userId, storedToken);

                return Ok(new
                {
                    message = "Token refreshed successfully",
                    accessToken = newTokenResponse.access_token,
                    expiresIn = newTokenResponse.expires_in
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new { error = "Failed to refresh token" });
            }
        }

        /// <summary>
        /// Revokes the stored token for a user
        /// </summary>
        [HttpDelete("revoke/{userId}")]
        public async Task<IActionResult> RevokeToken(string userId = "default")
        {
            try
            {
                await _tokenStorage.RemoveTokenAsync(userId);
                return Ok(new { message = "Token revoked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return StatusCode(500, new { error = "Failed to revoke token" });
            }
        }
    }
}
