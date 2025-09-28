namespace WebApplication1.Models;

public class AddUsersToSegmentRequest
{
    public List<UserData>? Users { get; set; }
    public string? SegmentId { get; set; }
}

public class UserData
{
    public List<string>? Schema { get; set; }
    public List<List<string>>? Data { get; set; }
}
