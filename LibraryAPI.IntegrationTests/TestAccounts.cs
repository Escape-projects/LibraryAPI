namespace LibraryAPI.IntegrationTests;

/// <summary>
/// Учётки, которые интеграционные тесты создают в тестовой базе.
/// Пароль один на всех: так тестам не нужно держать лишние константы.
/// </summary>
public static class TestAccounts
{
    public const string Password = "Passw0rd!";
    public const string AdminLogin = "it-admin";
    public const string LibrarianLogin = "it-librarian";
    public const string ReaderLogin = "it-reader";
}
