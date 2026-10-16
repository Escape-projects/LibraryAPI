using LibraryAPI.Contracts.DTOs.Account;
using LibraryAPI.Contracts.DTOs.Common;

namespace LibraryAPI.Services
{
    public interface IAccountService
    {
        Task<PagedResult<AccountDTO>> GetAccountsAsync(AccountFilterParams Filters);
        Task<AccountDTO> GetAccountAsync(int Id);
        Task<AccountDTO> CreateAccountAsync(AccountCreateDTO Account);
        Task UpdateAccountAsync(int Id, int CurrentAccountId, AccountUpdateDTO Account);
        Task DeleteAccountAsync(int Id, int CurrentAccountId);
    }
}
