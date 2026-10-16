using LibraryAPI.Contracts.DTOs.Common;

namespace LibraryAPI.Contracts.DTOs.Author
{
    public class AuthorFilterParams : PaginationParams
    {
        public string? name { get; set; }
        public string? country { get; set; }
        public int? booksCountMore { get; set; }
        public int? booksCountLess { get; set; }
        public string? sortBy { get; set; }
    }
}
