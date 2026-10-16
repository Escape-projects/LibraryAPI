using LibraryAPI.Contracts.DTOs.Account;
using LibraryAPI.Services;
using LibraryAPI.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService accounts;

        public AccountsController(IAccountService AccountService)
        {
            accounts = AccountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccounts([FromQuery] AccountFilterParams filters)
        {
            var result = await accounts.GetAccountsAsync(filters);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(int id)
        {
            var account = await accounts.GetAccountAsync(id);
            return Ok(account);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(AccountCreateDTO createAccount)
        {
            var createdAccount = await accounts.CreateAccountAsync(createAccount);
            return CreatedAtAction(nameof(GetAccount), new { createdAccount.id }, createdAccount);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, AccountUpdateDTO updateAccount)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentId)) throw new UnauthorizedException("Token does not contain the account id claim");
            await accounts.UpdateAccountAsync(id, currentId, updateAccount);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentId)) throw new UnauthorizedException("Token does not contain the account id claim");
            await accounts.DeleteAccountAsync(id, currentId);
            return NoContent();
        }
    }
}
