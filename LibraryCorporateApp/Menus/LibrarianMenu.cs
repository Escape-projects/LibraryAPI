namespace LibraryCorporateApp.Menus
{
    public class LibrarianMenu
    {
        private readonly BooksMenu booksMenu;

        public LibrarianMenu(BooksMenu BooksMenu)
        {
            booksMenu = BooksMenu;
        }

        public async Task Run()
        {
            await booksMenu.Open(true);
        }
    }
}
