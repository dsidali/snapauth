using System.Collections.Generic;
using WebApplication1.Models;

namespace WebApplication1.Services;

public interface ISnapchatSegmentService
{
    Task<string> CreateSegmentAsync(List<SnapchatSegment> segments, string token);
    Task<string> GetAllSegmentsAsync(string adAccountId, string token);
    Task<string> GetSegmentAsync(string segmentId, string token);
    Task<string> AddUsersToSegmentAsync(string segmentId, AddUsersToSegmentRequest request, string token);
    Task<string> RemoveAllUsersFromSegmentAsync(string segmentId, string token);
    Task<string> DeleteSegmentAsync(string segmentId, string token);
}
