using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Shared;

public class CategoryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int BookCount { get; set; }
    public List<Category> CategoryList { get; set; } = new();
    public Category NewCategory { get; set; } = new();
}