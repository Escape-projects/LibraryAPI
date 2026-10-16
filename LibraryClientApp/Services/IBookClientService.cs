using LibraryAPI.Contracts.DTOs.Book;

namespace LibraryClientApp.Services
{
    public interface IBookClientService
    {
        BookFilterParams FindByTitle();
        BookFilterParams FindByYear();
        BookFilterParams FindByAuthor();
    }
}
