using AutoMapper;
using LibraryAPI.Contracts.DTOs.Account;
using LibraryAPI.Contracts.DTOs.Author;
using LibraryAPI.Contracts.DTOs.Book;
using LibraryAPI.Models;

namespace LibraryAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Author, AuthorDTO>();
            CreateMap<Book, BookDTO>().ForMember(dest => dest.authorName, opt => opt.MapFrom(src => src.author.name));
            CreateMap<Account, AccountDTO>();
        }
    }
}
