using AutoMapper;
using LibraryAPI.Contracts.DTOs.Account;
using LibraryAPI.Contracts.DTOs.Common;
using LibraryAPI.Exceptions;
using LibraryAPI.Models;
using LibraryAPI.Services.Jwt;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Services.API
{
    public class AccountService : IAccountService
    {
        private readonly LibraryContext library;
        private readonly IMapper mapper;
        private readonly ILogger<AccountService> logger;
        private readonly IPasswordHasher hasher;
        private static readonly string[] StaffRoles = { "admin", "librarian" }; 

        public AccountService(LibraryContext Library, IMapper Mapper, ILogger<AccountService> Logger, IPasswordHasher passwordHasher)
        {
            library = Library;
            mapper = Mapper;
            logger = Logger;
            hasher = passwordHasher;
        }

        public async Task<PagedResult<AccountDTO>> GetAccountsAsync(AccountFilterParams filters)
        {
            var query = library.Accounts.AsNoTracking();
            query = query.Where(account => StaffRoles.Contains(account.role));
            if (filters.login != null) query = query.Where(account => account.login.Contains(filters.login));
            if (filters.role != null) query = query.Where(account => account.role == filters.role);
            switch (filters.sortBy)
            {
                case "login": query = query.OrderBy(account => account.login); break;
                case "login_desc": query = query.OrderByDescending(account => account.login); break;
                case "role": query = query.OrderBy(account => account.role); break;
                case "role_desc": query = query.OrderByDescending(account => account.role); break;
                default: query = query.OrderBy(account => account.id); break;
            }

            var totalCount = await query.CountAsync();
            var accounts = await query.Skip((filters.page - 1) * filters.pageSize).Take(filters.pageSize).ToListAsync();
            var accountsDTO = mapper.Map<List<AccountDTO>>(accounts);
            return new PagedResult<AccountDTO>(accountsDTO, totalCount, filters.page, filters.pageSize);
        }

        public async Task<AccountDTO> GetAccountAsync(int id)
        {
            var account = await library.Accounts.FirstOrDefaultAsync(account => account.id == id && StaffRoles.Contains(account.role));
            if (account == null) throw new NotFoundException($"Account(id:{id}) not found");
            return mapper.Map<AccountDTO>(account);
        }

        public async Task<AccountDTO> CreateAccountAsync(AccountCreateDTO newAccount)
        {
            logger.LogInformation("Creating account: {Login} ({Role})", newAccount.login, newAccount.role);

            if (await library.Accounts.AnyAsync(account => account.login == newAccount.login)) throw new BadRequestException($"Login({newAccount.login}) already taken");
            var account = new Account { login = newAccount.login, role = newAccount.role.ToLowerInvariant(), passwordHash = hasher.Hash(newAccount.password) };
            library.Accounts.Add(account);
            await library.SaveChangesAsync();

            logger.LogInformation("Account(id:{AccountId}) created", account.id);
            return mapper.Map<AccountDTO>(account);
        }

        public async Task UpdateAccountAsync(int id, int currentAccountId, AccountUpdateDTO updateAccount)
        {
            logger.LogInformation("Updating account(id:{AccountId}): {NewLogin} ({NewRole})", id, updateAccount.login, updateAccount.role);

            if (id == currentAccountId && updateAccount.role != "admin") throw new BadRequestException("Cannot demote yourself");
            var account = await library.Accounts.FirstOrDefaultAsync(account => account.id == id && StaffRoles.Contains(account.role));
            if (account == null) throw new NotFoundException($"Account(id:{id}) not found");
            if (await library.Accounts.AnyAsync(account => account.login == updateAccount.login && account.id != id)) throw new BadRequestException($"Login({updateAccount.login}) already taken");
            account.login = updateAccount.login;
            account.role = updateAccount.role.ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(updateAccount.password)) account.passwordHash = hasher.Hash(updateAccount.password);
            await library.SaveChangesAsync();

            logger.LogInformation("Account(id:{AccountId}) updated", account.id);
        }

        public async Task DeleteAccountAsync(int id, int currentAccountId)
        {
            logger.LogInformation("Deleting account(id:{AccountId})", id);

            if (id == currentAccountId) throw new BadRequestException("Cannot delete yourself");
            var account = await library.Accounts.FirstOrDefaultAsync(account => account.id == id && StaffRoles.Contains(account.role));
            if (account == null) throw new NotFoundException($"Account(id:{id}) not found");
            library.Accounts.Remove(account);
            await library.SaveChangesAsync();

            logger.LogInformation("Account(id:{AccountId}) deleted", id);
        }
    }
}
