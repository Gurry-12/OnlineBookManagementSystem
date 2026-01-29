using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Presentation.ViewModels.Books;

public class BookFormViewModel
{
    public Book? Book { get; set; }
    public List<SelectListItem> Categories { get; set; } = new();
    public IFormFile? ImageFile { get; set; }
    public bool IsEdit => Book?.Id > 0;
    public string FormTitle => IsEdit ? "Edit Book" : "Add New Book";
    public string SubmitButtonText => IsEdit ? "Update Book" : "Create Book";
}