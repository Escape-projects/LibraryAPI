namespace LibraryAPI.Models
{
    public class Author
    {
        public int id { get; set; }
        public string name { get; set; }
        public string country { get; set; }
        public List<Book> books { get; set; } = new List<Book>();
    }
}
