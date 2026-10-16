namespace LibraryAPI.Contracts.DTOs.Book
{
    public class BookDTO
    {
        public int id { get; set; }
        public string title { get; set; }
        public int year { get; set; }
        public int authorId { get; set; }
        public string authorName { get; set; }
    }
}
