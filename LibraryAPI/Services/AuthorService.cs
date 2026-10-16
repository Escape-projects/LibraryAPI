using AutoMapper;
using LibraryAPI.Contracts.DTOs.Author;
using LibraryAPI.Contracts.DTOs.Common;
using LibraryAPI.Exceptions;
using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly LibraryContext library;
        private readonly IMapper mapper;
        private readonly ILogger<AuthorService> logger;
        private readonly ICacheService cache;

        public AuthorService(LibraryContext Library, IMapper Mapper, ILogger<AuthorService> Logger, ICacheService cacheService)
        {
            library = Library;
            mapper = Mapper;
            logger = Logger;
            cache = cacheService;
        }

        public async Task<PagedResult<AuthorDTO>> GetAuthorsAsync(AuthorFilterParams filters)
        {
            var cacheKey = $"authors:page={filters.page}_size={filters.pageSize}" +
                           $"_name={filters.name}_country={filters.country}" +
                           $"_booksCountLess={filters.booksCountLess}_booksCountMore={filters.booksCountMore}" +
                           $"_sort={filters.sortBy}";

            var cached = await cache.GetAsync<PagedResult<AuthorDTO>>(cacheKey);
            if (cached != null)
            {
                logger.LogInformation("Cache HIT: {Key}", cacheKey);
                return cached;
            }
            logger.LogInformation("Cache MISS: {Key}", cacheKey);

            var query = library.Authors.AsNoTracking();
            if (filters.name != null) query = query.Where(author => author.name.Contains(filters.name));
            if (filters.country != null) query = query.Where(author => author.country.Contains(filters.country));
            if (filters.booksCountMore != null) query = query.Where(author => author.books.Count > filters.booksCountMore);
            if (filters.booksCountLess != null) query = query.Where(author => author.books.Count < filters.booksCountLess);
            IOrderedQueryable<Author> ordered = filters.sortBy switch
            {
                "name" => query.OrderBy(author => author.name),
                "name_desc" => query.OrderByDescending(author => author.name),
                "country" => query.OrderBy(author => author.country),
                "country_desc" => query.OrderByDescending(author => author.country),
                "booksCount" => query.OrderBy(author => author.books.Count),
                "booksCount_desc" => query.OrderByDescending(author => author.books.Count),
                _ => query.OrderBy(author => author.id)
            };
            query = ordered.ThenBy(author => author.id);

            var totalCount = await query.CountAsync();
            var authors = await query.Include(author => author.books).Skip((filters.page - 1) * filters.pageSize).Take(filters.pageSize).ToListAsync();
            var authorsDTO = mapper.Map<List<AuthorDTO>>(authors);
            var authorDto = new PagedResult<AuthorDTO>(authorsDTO, totalCount, filters.page, filters.pageSize);

            await cache.SetAsync(cacheKey, authorDto, TimeSpan.FromMinutes(5));

            return authorDto;
        }

        public async Task<AuthorDTO> GetAuthorAsync(int id)
        {
            var cacheKey = $"author:{id}";
            var cached = await cache.GetAsync<AuthorDTO>(cacheKey);
            if (cached != null)
            {
                logger.LogInformation("Cache HIT: {Key}", cacheKey);
                return cached;
            }
            logger.LogInformation("Cache MISS: {Key}", cacheKey);

            var author = await library.Authors.Include(author => author.books).FirstOrDefaultAsync(author => author.id == id);
            if (author == null) throw new NotFoundException($"Author(id:{id}) not found");
            var authorDTO = mapper.Map<AuthorDTO>(author);

            await cache.SetAsync(cacheKey, authorDTO, TimeSpan.FromMinutes(10));

            return authorDTO;
        }

        public async Task<AuthorDTO> GetAuthorAsync(string name)
        {
            var cacheKey = $"author:{name}";
            var cached = await cache.GetAsync<AuthorDTO>(cacheKey);
            if (cached != null)
            {
                logger.LogInformation("Cache HIT: {Key}", cacheKey);
                return cached;
            }
            logger.LogInformation("Cache MISS: {Key}", cacheKey);

            var author = await library.Authors.FirstOrDefaultAsync(author => author.name.Contains(name));
            if (author == null) throw new NotFoundException($"Author ({name}) not found");
            var authorDTO = mapper.Map<AuthorDTO>(author);

            await cache.SetAsync(cacheKey, authorDTO, TimeSpan.FromMinutes(10));

            return authorDTO;
        }

        public async Task<AuthorDTO> CreateAuthorAsync(AuthorCreateDTO newAuthor)
        {
            logger.LogInformation("Creating author: {Name} ({Country})", newAuthor.name, newAuthor.country);

            var author = new Author { name = newAuthor.name, country = newAuthor.country };
            library.Authors.Add(author);
            await library.SaveChangesAsync();
            await cache.RemoveByPrefixAsync("authors:");

            logger.LogInformation("Author(id:{AuthorId}) created", author.id);
            return mapper.Map<AuthorDTO>(author);
        }

        public async Task UpdateAuthorAsync(int id, AuthorUpdateDTO updateAuthor)
        {
            logger.LogInformation("Updating author(id:{AuthorId}): {NewName} ({NewCountry})", id, updateAuthor.name, updateAuthor.country);

            var author = await library.Authors.FirstOrDefaultAsync(author => author.id == id);
            if (author == null) throw new NotFoundException($"Author(id:{id}) not found");
            var oldName = author.name;
            author.name = updateAuthor.name;
            author.country = updateAuthor.country;
            await library.SaveChangesAsync();
            await cache.RemoveByPrefixAsync("authors:");
            await cache.RemoveByPrefixAsync("books:");
            await cache.RemoveAsync($"author:{oldName}");
            var authorDTO = mapper.Map<AuthorDTO>(author);
            await cache.SetAsync($"author:{id}", authorDTO, TimeSpan.FromMinutes(10));

            logger.LogInformation("Author(id:{AuthorId}) updated", id);
        }

        public async Task DeleteAuthorAsync(int id)
        {
            logger.LogInformation("Deleting author(id:{AuthorId})", id);

            var author = await library.Authors.Include(author => author.books).FirstOrDefaultAsync(author => author.id == id);
            if (author == null) throw new NotFoundException($"Author(id:{id} not found)");
            if (author.books.Count > 0) throw new ConflictException($"Author(id:{id} has books)");
            library.Authors.Remove(author);
            await library.SaveChangesAsync();
            await cache.RemoveByPrefixAsync("authors:");
            await cache.RemoveAsync($"author:{id}");
            await cache.RemoveAsync($"author:{author.name}");

            logger.LogInformation("Author(id:{AuthorId}) deleted", id);
        }
    }
}
