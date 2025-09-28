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

    public async Task<string> CreateSegmentAsync(string adAccountId, string name, string description, string token)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var requestContent = new
        {
            name,
            description,
            ad_account_id = adAccountId,
            source_type = "FIRST_PARTY",
            retention_in_days = 180
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

        var content = new StringContent(
            JsonSerializer.Serialize(new { users = request.Users }),
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
