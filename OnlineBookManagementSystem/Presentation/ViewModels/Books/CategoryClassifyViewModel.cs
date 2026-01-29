using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Books;

public class CategoryClassifyViewModel
{
    public required string CategoryName { get; set; }
    public List<Book> Books { get; set; }
}