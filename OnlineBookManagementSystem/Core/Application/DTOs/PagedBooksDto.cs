namespace OnlineBookManagementSystem.Core.Application.DTOs;

public class PagedBooksDto
{
    public List<BookDto> Books { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; } = 0;

    // Additional properties for compatibility
    public int Page { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public int CurrentPage { get; set; }
    public int TotalBooks { get; set; }

    // Parameterless constructor
    public PagedBooksDto() { }

    // Constructor for use cases
    public PagedBooksDto(
        IEnumerable<BookDto> books,
        int totalCount,
        int pageNumber,
        int pageSize,
        int totalPages)
    {
        Books = books.ToList();
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = totalPages;
        Page = pageNumber;
        CurrentPage = pageNumber;
        TotalBooks = totalCount;
        HasNextPage = pageNumber < totalPages;
        HasPreviousPage = pageNumber > 1;
    }
}
