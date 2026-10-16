using LibraryAPI.Contracts.DTOs.Author;
using LibraryAPI.Services.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorService authors;

        public AuthorsController(IAuthorService AuthorService)
        {
            authors = AuthorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuthors([FromQuery] AuthorFilterParams filters)
        {
            var result = await authors.GetAuthorsAsync(filters);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAuthor(int id)
        {
            var author = await authors.GetAuthorAsync(id);
            return Ok(author);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetAuthor(string name)
        {
            var author = await authors.GetAuthorAsync(name);
            return Ok(author);
        }

        [HttpPost]
        [Authorize(Roles = "librarian,admin")]
        public async Task<IActionResult> CreateAuthor(AuthorCreateDTO newAuthor)
        {
            var createdAuthor = await authors.CreateAuthorAsync(newAuthor);
            return CreatedAtAction(nameof(GetAuthor), new { createdAuthor.id }, createdAuthor);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "librarian,admin")]
        public async Task<IActionResult> UpdateAuthor(int id, AuthorUpdateDTO updateAuthor)
        {
            await authors.UpdateAuthorAsync(id, updateAuthor);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "librarian,admin")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            await authors.DeleteAuthorAsync(id);
            return NoContent();
        }
    }
}
