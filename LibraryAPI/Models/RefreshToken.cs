namespace LibraryAPI.Models
{
    public class RefreshToken
    {
        public int id { get; set; }
        public int accountId { get; set; }
        public string tokenHash { get; set; }
        public DateTime expireAt { get; set; }
        public DateTime createdAt { get; set; }
        public Account account { get; set; }
    }
}
