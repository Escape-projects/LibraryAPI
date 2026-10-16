using LibraryAPI.Contracts.DTOs.Book;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService books;

        public BooksController(IBookService BookService)
        {
            books = BookService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks([FromQuery] BookFilterParams filters)
        {
            var result = await books.GetBooksAsync(filters);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await books.GetBookAsync(id);
            return Ok(book);
        }

        [HttpPost]
        [Authorize(Roles = "librarian,admin")]
        public async Task<IActionResult> CreateBook(BookCreateDTO createBook)
        {
            var createdBook = await books.CreateBookAsync(createBook);
            return CreatedAtAction(nameof(GetBook), new { createdBook.id }, createdBook);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "librarian,admin")]
        public async Task<IActionResult> UpdateBook(int id, BookUpdateDTO updateBook)
        {
            await books.UpdateBookAsync(id, updateBook);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "librarian,admin")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            await books.DeleteBookAsync(id);
            return NoContent();
        }
    }
}
