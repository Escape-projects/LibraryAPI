using LibraryAPI.Contracts.DTOs.Author;

namespace LibraryCorporateApp.Services
{
    public interface IAuthorClientService
    {
        Task<AuthorDTO?> FindOrCreate(string name);
        Task<bool> Add();
        Task<bool> Update();
        Task<bool> Delete();
    }
}
