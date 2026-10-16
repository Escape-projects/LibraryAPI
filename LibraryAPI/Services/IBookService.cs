using LibraryAPI.Contracts.DTOs.Book;
using LibraryAPI.Contracts.DTOs.Common;

namespace LibraryAPI.Services
{
    public interface IBookService
    {
        Task<PagedResult<BookDTO>> GetBooksAsync(BookFilterParams Filters);
        Task<BookDTO> GetBookAsync(int Id);
        Task<BookDTO> CreateBookAsync(BookCreateDTO Book);
        Task UpdateBookAsync(int Id, BookUpdateDTO Book);
        Task DeleteBookAsync(int Id);
    }
}
