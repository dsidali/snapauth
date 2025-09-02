namespace WebApplication1.Models
{
    public class StoredToken
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string UserId { get; set; }
    }
}
