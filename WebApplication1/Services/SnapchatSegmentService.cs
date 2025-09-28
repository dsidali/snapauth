using System.Linq;
using System.Text;
using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class SnapchatSegmentService : ISnapchatSegmentService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string BaseUrl = "https://adsapi.snapchat.com/v1";

    public SnapchatSegmentService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> CreateSegmentAsync(List<SnapchatSegment> segments, string token)
    {
        var httpClient = _httpClientFactory.CreateClient();

        // Use the first segment to get the ad account ID for the URL
        var adAccountId = segments.FirstOrDefault()?.AdAccountId;
        if (string.IsNullOrEmpty(adAccountId))
        {
            throw new ArgumentException("AdAccountId is required in at least one segment");
        }

        var requestContent = new
        {
            segments = segments.Select(s => new
            {
                name = s.Name,
                description = s.Description,
                ad_account_id = s.AdAccountId,
                source_type = s.SourceType ?? "FIRST_PARTY",
                retention_in_days = s.RetentionInDays
            }).ToList()
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestContent),
            Encoding.UTF8,
            "application/json");

        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.PostAsync($"{BaseUrl}/adaccounts/{adAccountId}/segments", content);

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> GetAllSegmentsAsync(string adAccountId, string token)
    {
        var httpClient = _httpClientFactory.CreateClient();

        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.GetAsync($"{BaseUrl}/adaccounts/{adAccountId}/segments");

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> GetSegmentAsync(string segmentId, string token)
    {
        var httpClient = _httpClientFactory.CreateClient();

        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.GetAsync($"{BaseUrl}/segments/{segmentId}");

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> AddUsersToSegmentAsync(string segmentId, AddUsersToSegmentRequest request, string token)
    {
        var httpClient = _httpClientFactory.CreateClient();

        // Transform the data to match the expected API format
        var formattedUsers = request.Users?.Select(user => new
        {
            schema = user.Schema,
            data = user.Data?.Select(row => row.ToArray()).ToArray()
        }).ToList();

        var content = new StringContent(
            JsonSerializer.Serialize(new { users = formattedUsers }),
            Encoding.UTF8,
            "application/json");

        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.PostAsync($"{BaseUrl}/segments/{segmentId}/users", content);

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> RemoveAllUsersFromSegmentAsync(string segmentId, string token)
    {
        var httpClient = _httpClientFactory.CreateClient();

        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.DeleteAsync($"{BaseUrl}/segments/{segmentId}/all_users");

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> DeleteSegmentAsync(string segmentId, string token)
    {
        var httpClient = _httpClientFactory.CreateClient();

        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.DeleteAsync($"{BaseUrl}/segments/{segmentId}");

        return await response.Content.ReadAsStringAsync();
    }
}
