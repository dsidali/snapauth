using System.Collections.Generic;

namespace WebApplication1.Models;

public class SnapchatSegmentRequest
{
    public List<SnapchatSegment>? Segments { get; set; }
}

public class SnapchatSegment
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? SourceType { get; set; }
    public int RetentionInDays { get; set; }
    public string? AdAccountId { get; set; }
}
