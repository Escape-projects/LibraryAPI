namespace LibraryAPI.Models
{
    public class Book
    {
        public int id { get; set; }
        public string title { get; set; }
        public int year { get; set; }
        public int authorId { get; set; }
        public Author author { get; set; }
    }
}
