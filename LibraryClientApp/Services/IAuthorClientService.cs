using LibraryAPI.Contracts.DTOs.Author;

namespace LibraryClientApp.Services
{
    public interface IAuthorClientService
    {
        AuthorFilterParams FindByName();
        AuthorFilterParams FindByCountry();
        AuthorFilterParams FindByBooksCount();
    }
}
