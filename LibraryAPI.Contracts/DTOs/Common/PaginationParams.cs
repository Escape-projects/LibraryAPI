namespace LibraryAPI.Contracts.DTOs.Common
{
    public class PaginationParams
    {
        private const int maxPageSize = 100;
        private int specifiedPageSize = 20;
        private int specifiedPage = 1;

        public int page { get => specifiedPage; set => specifiedPage = value < 1 ? 1 : value; }
        public int pageSize { get => specifiedPageSize; set => specifiedPageSize = Math.Clamp(value, 1, maxPageSize); }
    }
}
