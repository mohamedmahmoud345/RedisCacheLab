using Microsoft.EntityFrameworkCore;
using RedisCacheLab.Context;
using RedisCacheLab.Models;
using RedisCacheLab.Repositories.Interfaces;

namespace RedisCacheLab.Repositories;

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;
    public BookRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddBookAsync(Book book)
    {
        await _context.Books.AddAsync(book);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Book>> GetAllBooksAsync()
    {
        return await _context.Books.ToListAsync();
    }

    public async Task<Book?> GetBookByIsbnAsync(string isbn)
    {
        return await _context.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);
    }

    public async Task UpdateBookAsync(Book book)
    {
        _context.Books.Update(book);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteBookAsync(string isbn)
    {
        var book = await GetBookByIsbnAsync(isbn);
        if (book != null)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<(List<Book> Result,int TotalCount)> GetPaginatedAsync(int page, int pageSize)
    {
        var pageNumbers = (page - 1) * pageSize;

        var data = _context.Books.AsQueryable();

        var result = await data.Skip(pageNumbers)
            .Take(pageSize)
            .ToListAsync();

        return (Result: result,TotalCount: data.ToList().Count);
    }
}

