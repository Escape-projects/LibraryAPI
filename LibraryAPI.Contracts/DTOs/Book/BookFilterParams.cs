using LibraryAPI.Contracts.DTOs.Common;

namespace LibraryAPI.Contracts.DTOs.Book
{
    public class BookFilterParams : PaginationParams
    {
        public int? authorId { get; set; }
        public string? authorName { get; set; }
        public int? yearFrom { get; set; }
        public int? yearTo { get; set; }
        public string? title { get; set; }
        public string? sortBy { get; set; }
    }
}
