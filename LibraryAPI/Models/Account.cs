namespace LibraryAPI.Models
{
    public class Account
    {
        public int id { get; set; }
        public string login { get; set; }
        public string passwordHash { get; set; }
        public string role { get; set; } = "user";
    }
}
