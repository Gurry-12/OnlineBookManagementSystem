namespace OnlineBookManagementSystem.Core.Application.DTOs;

public class BookSearchDto
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }
    public int PageNumber { get; set; } = 1;
    public int Page => PageNumber; // Alias for compatibility
    public int PageSize { get; set; } = 10;

    // Constructor for compatibility
    public BookSearchDto(string? searchTerm)
    {
        SearchTerm = searchTerm;
    }

    public BookSearchDto(string? searchTerm, int? categoryId, decimal? minPrice1, string? sortBy, int page, int pageSize, decimal? minPrice = null, decimal? maxPrice = null)
    {
        SearchTerm = searchTerm;
        CategoryId = categoryId;
        SortBy = sortBy;
        PageNumber = page;
        PageSize = pageSize;
        MinPrice = minPrice;
        MaxPrice = maxPrice;
    }
}
