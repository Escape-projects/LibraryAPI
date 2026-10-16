namespace LibraryClientApp.Services
{
    public interface IAuthClientService
    {
        Task Login();
        Task<bool> Register();
    }
}
