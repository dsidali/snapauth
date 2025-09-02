namespace WebApplication1.Models
{
    public class SnapchatConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public string Scope { get; set; } = "snapchat-marketing-api";
    }
}
