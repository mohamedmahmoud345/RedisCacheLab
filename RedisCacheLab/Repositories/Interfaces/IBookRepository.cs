using RedisCacheLab.Models;

namespace RedisCacheLab.Repositories.Interfaces;

public class PaginatedResult<T>
{
    public List<T> Data { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
};

public interface IBookRepository
{
    Task AddBookAsync(Book book);
    Task<List<Book>> GetAllBooksAsync();
    Task<Book?> GetBookByIsbnAsync(string isbn);
    Task UpdateBookAsync(Book book);
    Task DeleteBookAsync(string isbn);
    Task<(List<Book> Result, int TotalCount)> GetPaginatedAsync(int page, int pageSize);
}
