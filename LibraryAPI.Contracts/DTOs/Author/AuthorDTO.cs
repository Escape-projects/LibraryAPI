using LibraryAPI.Contracts.DTOs.Book;

namespace LibraryAPI.Contracts.DTOs.Author
{
    public class AuthorDTO
    {
        public int id { get; set; }
        public string name { get; set; }
        public string country { get; set; }
        public List<BookDTO> books { get; set; } = new List<BookDTO>();
    }
}
