using Microsoft.AspNetCore.Mvc;
using RedisCacheLab.Models;
using RedisCacheLab.Repositories.Interfaces;
using RedisCacheLab.Services.Interfaces;

namespace RedisCacheLab.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookController(IBookRepository repository, ICacheService cache) : ControllerBase
{
    private const string ListCachePrefix = "books:page:";
    private static string ListCacheKey(int page, int pageSize) => $"books:page:{page}:size:{pageSize}";
    private static string BookCacheKey(string isbn) => $"books:isbn:{isbn}";

    [HttpGet]
    public async Task<IActionResult> GetAllBooks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var cacheKey = ListCacheKey(page, pageSize);

        var cached = await cache.GetAsync<PaginatedResult<Book>>(cacheKey);
        if (cached is not null)
        {
            Response.Headers.Append("X-Cache", "HIT");
            return Ok(cached);
        }

        var books = await repository.GetPaginatedAsync(page, pageSize);

        var result = new PaginatedResult<Book>
        {
            Data = books.Result,
            Page = page,
            PageSize = pageSize,
            TotalCount = books.TotalCount
        };

        await cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));

        Response.Headers.Append("X-Cache", "MISS");
        return Ok(result);
    }

    [HttpGet("{isbn}")]
    public async Task<IActionResult> GetBook(string isbn)
    {
        var cacheKey = BookCacheKey(isbn);

        var cached = await cache.GetAsync<Book>(cacheKey);
        if (cached is not null)
        {
            Response.Headers.Append("X-Cache", "HIT");
            return Ok(cached);
        }

        var book = await repository.GetBookByIsbnAsync(isbn);
        if (book is null)
        {
            return NotFound();
        }

        await cache.SetAsync(cacheKey, book, TimeSpan.FromMinutes(30));
        Response.Headers.Append("X-Cache", "MISS");

        return Ok(book);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBook([FromBody] Book book)
    {
        if (book is null)
        {
            return BadRequest();
        }

        await repository.AddBookAsync(book);

        await cache.RemoveByPatternAsync(ListCachePrefix);
        await cache.RemoveAsync(BookCacheKey(book.ISBN));

        return CreatedAtAction(nameof(GetBook), new { isbn = book.ISBN }, book);
    }

    [HttpPut("{isbn}")]
    public async Task<IActionResult> UpdateBook(string isbn, [FromBody] Book book)
    {
        if (isbn != book.ISBN)
        {
            return BadRequest("The ISBN in the URL does not match the ISBN in the body.");
        }

        var existingBook = await repository.GetBookByIsbnAsync(isbn);
        if (existingBook is null)
        {
            return NotFound();
        }

        await repository.UpdateBookAsync(book);

        await cache.RemoveAsync(BookCacheKey(isbn));
        await cache.RemoveByPatternAsync(ListCachePrefix);

        return NoContent();
    }

    [HttpDelete("{isbn}")]
    public async Task<IActionResult> DeleteBook(string isbn)
    {
        var existingBook = await repository.GetBookByIsbnAsync(isbn);
        if (existingBook is null)
        {
            return NotFound();
        }

        await repository.DeleteBookAsync(isbn);

        await cache.RemoveAsync(BookCacheKey(isbn));
        await cache.RemoveByPatternAsync(ListCachePrefix);

        return NoContent();
    }
}
