using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SnapchatController : ControllerBase
{
    private readonly ISnapchatSegmentService _snapchatSegmentService;
    private readonly ISnapchatAuthService _snapchatAuthService;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string DefaultUserId = "default";

    public SnapchatController(
        ISnapchatSegmentService snapchatSegmentService,
        ISnapchatAuthService snapchatAuthService,
        IHttpClientFactory httpClientFactory)
    {
        _snapchatSegmentService = snapchatSegmentService;
        _snapchatAuthService = snapchatAuthService;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("segments")]
    public async Task<IActionResult> CreateSegment([FromBody] SnapchatSegmentRequest request)
    {
        if (request == null || request.Segments == null || request.Segments.Count == 0)
        {
            return BadRequest(new { error = "Request body with segments is required." });
        }

        try
        {
            var storedToken = await _snapchatAuthService.GetValidTokenAsync(DefaultUserId);
            var result = await _snapchatSegmentService.CreateSegmentAsync(
                request.Segments,
                storedToken.AccessToken);

            return Content(result, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpGet("adaccounts/{adAccountId}/segments")]
    public async Task<IActionResult> GetAllSegments(string adAccountId)
    {
        if (string.IsNullOrWhiteSpace(adAccountId))
        {
            return BadRequest(new { error = "AdAccountId is required." });
        }

        try
        {
            var storedToken = await _snapchatAuthService.GetValidTokenAsync(DefaultUserId);
            var result = await _snapchatSegmentService.GetAllSegmentsAsync(adAccountId, storedToken.AccessToken);

            return Content(result, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpGet("segments/{segmentId}")]
    public async Task<IActionResult> GetSegment(string segmentId)
    {
        if (string.IsNullOrWhiteSpace(segmentId))
        {
            return BadRequest(new { error = "SegmentId is required." });
        }

        try
        {
            var storedToken = await _snapchatAuthService.GetValidTokenAsync(DefaultUserId);
            var result = await _snapchatSegmentService.GetSegmentAsync(segmentId, storedToken.AccessToken);

            return Content(result, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpPost("segments/{segmentId}/users")]
    public async Task<IActionResult> AddUsersToSegment(string segmentId, [FromBody] AddUsersToSegmentRequest request)
    {
        if (string.IsNullOrWhiteSpace(segmentId))
        {
            return BadRequest(new { error = "SegmentId is required." });
        }

        if (request == null || request.Users == null || request.Users.Count == 0)
        {
            return BadRequest(new { error = "Users data is required." });
        }

        try
        {
            request.SegmentId = segmentId ?? throw new ArgumentNullException(nameof(segmentId));
            var storedToken = await _snapchatAuthService.GetValidTokenAsync(DefaultUserId);
            var result = await _snapchatSegmentService.AddUsersToSegmentAsync(segmentId, request, storedToken.AccessToken);

            return Content(result, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpDelete("segments/{segmentId}/all_users")]
    public async Task<IActionResult> RemoveAllUsersFromSegment(string segmentId)
    {
        if (string.IsNullOrWhiteSpace(segmentId))
        {
            return BadRequest(new { error = "SegmentId is required." });
        }

        try
        {
            var storedToken = await _snapchatAuthService.GetValidTokenAsync(DefaultUserId);
            var result = await _snapchatSegmentService.RemoveAllUsersFromSegmentAsync(segmentId, storedToken.AccessToken);

            return Content(result, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpDelete("segments/{segmentId}")]
    public async Task<IActionResult> DeleteSegment(string segmentId)
    {
        if (string.IsNullOrWhiteSpace(segmentId))
        {
            return BadRequest(new { error = "SegmentId is required." });
        }

        try
        {
            var storedToken = await _snapchatAuthService.GetValidTokenAsync(DefaultUserId);
            var result = await _snapchatSegmentService.DeleteSegmentAsync(segmentId, storedToken.AccessToken);

            return Content(result, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        try
        {
            var storedToken = await _snapchatAuthService.GetValidTokenAsync(DefaultUserId);
            var httpClient = _httpClientFactory.CreateClient();

            // Set the authorization header with the Bearer token
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", storedToken.AccessToken);

            // Make the request to the Snapchat API
            var response = await httpClient.GetAsync("https://adsapi.snapchat.com/v1/me");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { error = errorContent });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpGet("organizations")]
    public async Task<IActionResult> GetOrganizations([FromQuery] bool withAdAccounts = true)
    {
        try
        {
            var storedToken = await _snapchatAuthService.GetValidTokenAsync(DefaultUserId);
            var httpClient = _httpClientFactory.CreateClient();

            // Set the authorization header with the Bearer token
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", storedToken.AccessToken);

            // Build the URL with query parameters
            var url = $"https://adsapi.snapchat.com/v1/me/organizations?with_ad_accounts={withAdAccounts.ToString().ToLower()}";

            // Make the request to the Snapchat API
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { error = errorContent });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpGet("organizations/{organizationId}")]
    public async Task<IActionResult> GetOrganization(string organizationId)
    {
        if (string.IsNullOrWhiteSpace(organizationId))
        {
            return BadRequest(new { error = "OrganizationId is required." });
        }

        try
        {
            var storedToken = await _snapchatAuthService.GetValidTokenAsync(DefaultUserId);
            var httpClient = _httpClientFactory.CreateClient();

            // Set the authorization header with the Bearer token
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", storedToken.AccessToken);

            // Build the URL with the organization ID
            var url = $"https://adsapi.snapchat.com/v1/organizations/{organizationId}";

            // Make the request to the Snapchat API
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { error = errorContent });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpGet("organizations/{organizationId}/adaccounts")]
    public async Task<IActionResult> GetAdAccounts(string organizationId)
    {
        if (string.IsNullOrWhiteSpace(organizationId))
        {
            return BadRequest(new { error = "OrganizationId is required." });
        }

        try
        {
            var storedToken = await _snapchatAuthService.GetValidTokenAsync(DefaultUserId);
            var httpClient = _httpClientFactory.CreateClient();

            // Set the authorization header with the Bearer token
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", storedToken.AccessToken);

            // Build the URL with the organization ID
            var url = $"https://adsapi.snapchat.com/v1/organizations/{organizationId}/adaccounts";

            // Make the request to the Snapchat API
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { error = errorContent });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }
}
