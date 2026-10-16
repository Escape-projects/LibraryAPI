namespace LibraryCorporateApp.Services
{
    public interface IBookClientService
    {
        Task<bool> Add();
        Task<bool> Update();
        Task<bool> Delete();
    }
}
