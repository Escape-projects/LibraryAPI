namespace LibraryClientApp
{
    public static class Helpers
    {
        public static void PressAnyKeyToContinue()
        {
            Console.WriteLine("***нажмите любую кнопку чтобы продолжить");
            Console.ReadKey();
        }

        public static string Truncate(string str, int maxLength)
        {
            return str.Length > maxLength ? str.Substring(0, maxLength - 3) + "..." : str;
        }
    }
}
