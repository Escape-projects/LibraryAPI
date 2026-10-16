using LibraryAPI.Contracts.DTOs.Account;
using LibraryAPI.Exceptions;
using LibraryAPI.Models;
using LibraryAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryAPI.Tests
{
    public class AccountServiceTests : IClassFixture<MappingFixture>
    {
        private readonly MappingFixture fixture;

        private LibraryContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new LibraryContext(options);
        }

        public AccountServiceTests(MappingFixture Fixture)
        {
            fixture = Fixture;
        }

        private AccountService CreateService(LibraryContext library)
        {
            var mapper = fixture.mapper;
            var logger = new Mock<ILogger<AccountService>>().Object;
            var hasherMock = new Mock<IPasswordHasher>();
            hasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("password hash");
            return new AccountService(library, mapper, logger, hasherMock.Object);
        }

        [Fact]
        public async Task GetAccountsAsync_HidesUserRole()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "аккаунт1", passwordHash = "password hash", role = "admin"});
            library.Accounts.Add(new Account { id = 2, login = "аккаунт2", passwordHash = "password hash", role = "librarian" });
            library.Accounts.Add(new Account { id = 3, login = "аккаунт3", passwordHash = "password hash", role = "user" });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            var result = await service.GetAccountsAsync(new AccountFilterParams());
            Assert.NotNull(result);
            Assert.True(result.items.Count == 2);
            Assert.Null(result.items.FirstOrDefault(account => account.role == "user"));
        }

        [Fact]
        public async Task GetAccountsAsync_FilterByRole_ReturnsOnlyThatRole()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "аккаунт1", passwordHash = "password hash", role = "admin" });
            library.Accounts.Add(new Account { id = 2, login = "аккаунт2", passwordHash = "password hash", role = "librarian" });
            library.Accounts.Add(new Account { id = 3, login = "аккаунт3", passwordHash = "password hash", role = "admin" });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            var result = await service.GetAccountsAsync(new AccountFilterParams { role = "admin" });
            Assert.NotNull(result);
            Assert.True(result.items.Count == 2);
            Assert.Null(result.items.FirstOrDefault(account => account.role == "librarian"));
        }

        [Fact]
        public async Task CreateAccountAsync_ValidDto_CreatesWithHashedPassword()
        {
            using var library = GetInMemoryContext();
            var service = CreateService(library);

            var result = await service.CreateAccountAsync(new AccountCreateDTO { login = "аккаунт", password = "123456", role = "admin"});
            Assert.NotNull(result);
            Assert.True(library.Accounts.First().passwordHash == "password hash");
        }

        [Fact]
        public async Task CreateAccountAsync_LoginTaken_ThrowsBadRequest()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "аккаунт", passwordHash = "password hash", role = "admin"});
            await library.SaveChangesAsync();
            var service = CreateService(library);

            await Assert.ThrowsAsync<BadRequestException>(() => service.CreateAccountAsync(new AccountCreateDTO { login = "аккаунт", password = "123456", role = "librarian" }));
        }

        [Fact]
        public async Task UpdateAccountAsync_DemoteSelf_ThrowsBadRequest()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "аккаунт", passwordHash = "password hash", role = "admin" });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateAccountAsync(1, 1, new AccountUpdateDTO { login = "аккаунт", role = "librarian" }));
        }

        [Fact]
        public async Task UpdateAccountAsync_UpdateSelfToAdmin_Ok()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "аккаунт", passwordHash = "password hash", role = "admin" });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            var exception = await Record.ExceptionAsync(() => service.UpdateAccountAsync(1, 1, new AccountUpdateDTO { login = "аккаунт", role = "admin" }));
            Assert.Null(exception);
        }

        [Fact]
        public async Task UpdateAccountAsync_UpdateAnotherUser_Ok()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "аккаунт1", passwordHash = "password hash", role = "admin" });
            library.Accounts.Add(new Account { id = 2, login = "аккаунт2", passwordHash = "password hash", role = "librarian" });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            var exception = await Record.ExceptionAsync(() => service.UpdateAccountAsync(2, 1, new AccountUpdateDTO { login = "аккаунт", role = "admin" }));
            Assert.Null(exception);
        }

        [Fact]
        public async Task UpdateAccountAsync_PasswordNull_DoesNotChangePassword()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "аккаунт", passwordHash = "old password hash", role = "admin" });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            await service.UpdateAccountAsync(1, 1, new AccountUpdateDTO { login = "аккаунт", password = null, role = "admin" });
            Assert.Equal("old password hash", library.Accounts.First().passwordHash);
        }

        [Fact]
        public async Task DeleteAccountAsync_DeleteSelf_ThrowsBadRequest()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "аккаунт", passwordHash = "password hash", role = "admin" });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            await Assert.ThrowsAsync<BadRequestException>(() => service.DeleteAccountAsync(1, 1));
            Assert.Equal(1, library.Accounts.Count());
        }

        [Fact]
        public async Task DeleteAccountAsync_DeleteAnother_Ok()
        {
            using var library = GetInMemoryContext();
            library.Accounts.Add(new Account { id = 1, login = "аккаунт1", passwordHash = "password hash", role = "admin" });
            library.Accounts.Add(new Account { id = 2, login = "аккаунт2", passwordHash = "password hash", role = "librarian" });
            await library.SaveChangesAsync();
            var service = CreateService(library);

            var exception = await Record.ExceptionAsync(() => service.DeleteAccountAsync(2, 1));
            Assert.Null(exception);
            Assert.Equal(1, library.Accounts.Count());
        }
    }
}
