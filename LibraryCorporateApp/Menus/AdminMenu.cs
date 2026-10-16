namespace LibraryCorporateApp.Menus
{
    public class AdminMenu
    {
        private readonly AccountsMenu accountsMenu;

        public AdminMenu(AccountsMenu AccountsMenu)
        {
            accountsMenu = AccountsMenu;
        }

        public async Task Run()
        {
            await accountsMenu.Open();
        }
    }
}
