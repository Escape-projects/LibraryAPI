using LibraryAPI.Contracts.DTOs.Common;

namespace LibraryAPI.Contracts.DTOs.Account
{
    public class AccountFilterParams : PaginationParams
    {
        public string? login { get; set; }
        public string? role { get; set; }
        public string? sortBy { get; set; }
    }
}
