using AutoMapper;
using LibraryAPI.Contracts.DTOs.Book;
using LibraryAPI.Contracts.DTOs.Common;
using LibraryAPI.Exceptions;
using LibraryAPI.Models;
using LibraryAPI.Services.Redis;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Services.API
{
    public class BookService : IBookService
    {
        private readonly LibraryContext library;
        private readonly IMapper mapper;
        private readonly ILogger<BookService> logger;
        private readonly ICacheService cache;

        public BookService(LibraryContext Library, IMapper Mapper, ILogger<BookService> Logger, ICacheService cacheService)
        {
            library = Library;
            mapper = Mapper;
            logger = Logger;
            cache = cacheService;
        }

        public async Task<PagedResult<BookDTO>> GetBooksAsync(BookFilterParams filters)
        {
            var cacheKey = $"books:page={filters.page}_size={filters.pageSize}" +
                           $"_authorId={filters.authorId}_authorName={filters.authorName}" + 
                           $"_yearFrom={filters.yearFrom}_yearTo={filters.yearTo}" +
                           $"_title={filters.title}_sort={filters.sortBy}";

            var cached = await cache.GetAsync<PagedResult<BookDTO>>(cacheKey);
            if (cached != null)
            {
                logger.LogInformation("Cache HIT: {Key}", cacheKey);
                return cached;
            }
            logger.LogInformation("Cache MISS: {Key}", cacheKey);

            var query = library.Books.AsNoTracking();
            if (filters.authorId != null) query = query.Where(book => book.authorId == filters.authorId);
            if (filters.authorName != null) query = query.Include(book => book.author).Where(book => book.author.name.Contains(filters.authorName));
            if (filters.yearFrom != null) query = query.Where(book => book.year >= filters.yearFrom);
            if (filters.yearTo != null) query = query.Where(book => book.year <= filters.yearTo);
            if (filters.title != null) query = query.Where(book => book.title.Contains(filters.title));
            IOrderedQueryable<Book> ordered = filters.sortBy switch
            {
                "title" => query.OrderBy(book => book.title),
                "title_desc" => query.OrderByDescending(book => book.title),
                "year" => query.OrderBy(book => book.year),
                "year_desc" => query.OrderByDescending(book => book.year),
                "authorId" => query.OrderBy(book => book.authorId),
                "authorId_desc" => query.OrderByDescending(book => book.authorId),
                "authorName" => query.Include(book => book.author).OrderBy(book => book.author.name),
                "authorName_desc" => query.Include(book => book.author).OrderByDescending(book => book.author.name),
                _ => query.OrderBy(book => book.id)
            };
            query = ordered.ThenBy(book => book.id);

            var totalCount = await query.CountAsync();
            var books = await query.Include(book => book.author).Skip((filters.page - 1) * filters.pageSize).Take(filters.pageSize).ToListAsync();
            var booksDTO = mapper.Map<List<BookDTO>>(books);
            var result = new PagedResult<BookDTO>(booksDTO, totalCount, filters.page, filters.pageSize);

            await cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }

        public async Task<BookDTO> GetBookAsync(int id)
        {
            var cacheKey = $"books:id={id}";
            var cached = await cache.GetAsync<BookDTO>(cacheKey);
            if (cached != null)
            {
                logger.LogInformation("Cache HIT: {Key}", cacheKey);
                return cached;
            }
            logger.LogInformation("Cache MISS: {Key}", cacheKey);

            var book = await library.Books.Include(book => book.author).FirstOrDefaultAsync(book => book.id == id);
            if (book == null) throw new NotFoundException($"Book(id:{id}) not found");
            var bookDTO = mapper.Map<BookDTO>(book);
            await cache.SetAsync(cacheKey, bookDTO, TimeSpan.FromMinutes(10));
            return bookDTO;
        }

        public async Task<BookDTO> CreateBookAsync(BookCreateDTO newBook)
        {
            logger.LogInformation("Creating book: {Title} ({Year}) by authorId: {AuthorId}", newBook.title, newBook.year, newBook.authorId);

            var author = await library.Authors.FindAsync(newBook.authorId);
            if (author == null) throw new NotFoundException($"Author(id:{newBook.authorId}) not found");

            var book = new Book { title = newBook.title, year = newBook.year, authorId = author.id, author = author };
            library.Books.Add(book);
            await library.SaveChangesAsync();
            await cache.RemoveByPrefixAsync("books:");
            await cache.RemoveByPrefixAsync("authors:");

            logger.LogInformation("Book(id:{BookId}) created", book.id);
            return mapper.Map<BookDTO>(book);
        }

        public async Task UpdateBookAsync(int id, BookUpdateDTO updateBook)
        {
            logger.LogInformation("Updating book(id:{BookId}): {NewTitle} ({NewYear}) by author {NewAuthorId}", id, updateBook.title, updateBook.year, updateBook.authorId);

            var book = await library.Books.FirstOrDefaultAsync(book => book.id == id);
            if (book == null) throw new NotFoundException($"Book(id:{id}) not found");
            if (!await library.Authors.AnyAsync(author => author.id == updateBook.authorId)) throw new NotFoundException($"Author(id:{updateBook.authorId}) not found");
            book.title = updateBook.title;
            book.year = updateBook.year;
            book.authorId = updateBook.authorId;
            await library.SaveChangesAsync();
            await cache.RemoveByPrefixAsync("books:");
            await cache.RemoveByPrefixAsync("authors:");

            logger.LogInformation("Book(id:{BookId}) updated", book.id);
        }

        public async Task DeleteBookAsync(int id)
        {
            logger.LogInformation("Deleting book(id:{BookId})", id);

            var book = await library.Books.FirstOrDefaultAsync(book => book.id == id);
            if (book == null) throw new NotFoundException($"Book(id:{id}) not found");
            library.Books.Remove(book);
            await library.SaveChangesAsync();
            await cache.RemoveByPrefixAsync("books:");
            await cache.RemoveByPrefixAsync("authors:");

            logger.LogInformation("Book(id:{BookId}) deleted", id);
        }
    }
}
