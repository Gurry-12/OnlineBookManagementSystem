using FluentValidation;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Validation
{
    public class BookViewModelValidator : AbstractValidator<BookViewModel>
    {
        public BookViewModelValidator()
        {
            RuleFor(book => book.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(255).WithMessage("Title cannot exceed 255 characters.");

            RuleFor(book => book.Author)
                .MaximumLength(100).WithMessage("Author name cannot exceed 100 characters.");

            RuleFor(book => book.Isbn)
                .MaximumLength(100).WithMessage("ISBN cannot exceed 100 characters.")
                .Must(isbn => string.IsNullOrEmpty(isbn) || isbn.All(char.IsLetterOrDigit))
                .WithMessage("ISBN must differ from standard (optional check).");

            RuleFor(book => book.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

            RuleFor(book => book.Stock)
                .NotEmpty().WithMessage("Stock information is required.");
        }
    }
}
