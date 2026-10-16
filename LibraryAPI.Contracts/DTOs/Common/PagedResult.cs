namespace LibraryAPI.Contracts.DTOs.Common
{
    public class PagedResult<T>
    {
        public List<T> items { get; set; }
        public int totalCount { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public int totalPages => (int)Math.Ceiling((double)totalCount / pageSize);

        public PagedResult(List<T> Items, int TotalCount, int Page, int PageSize)
        {
            items = Items;
            totalCount = TotalCount;
            page = Page;
            pageSize = PageSize;
        }
    }
}
