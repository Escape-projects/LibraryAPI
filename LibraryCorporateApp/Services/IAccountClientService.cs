namespace LibraryCorporateApp.Services
{
    public interface IAccountClientService
    {
        Task<bool> Add();
        Task<bool> Update();
        Task<bool> Delete();
    }
}
