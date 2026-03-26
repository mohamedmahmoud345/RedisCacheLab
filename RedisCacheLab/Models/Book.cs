namespace RedisCacheLab.Models;

public class Book
{
    public Book(int id, string title, string author, string iSBN)
    {
        Id = id;
        Title = title;
        Author = author;
        ISBN = iSBN;
        IsAvailable = true;
    }
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string ISBN { get; set; }
    public bool IsAvailable { get; set; }
}
