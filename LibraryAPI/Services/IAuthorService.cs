using LibraryAPI.Contracts.DTOs.Author;
using LibraryAPI.Contracts.DTOs.Common;

namespace LibraryAPI.Services
{
    public interface IAuthorService
    {
        Task<PagedResult<AuthorDTO>> GetAuthorsAsync(AuthorFilterParams Filters);
        Task<AuthorDTO> GetAuthorAsync(int Id);
        Task<AuthorDTO> GetAuthorAsync(string Name);
        Task<AuthorDTO> CreateAuthorAsync(AuthorCreateDTO Author);
        Task UpdateAuthorAsync(int Id, AuthorUpdateDTO Author);
        Task DeleteAuthorAsync(int Id);
    }
}
