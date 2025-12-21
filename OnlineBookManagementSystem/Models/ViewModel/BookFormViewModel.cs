using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class BookFormViewModel
    {
        public BookViewModel Book { get; set; }

        public IEnumerable<SelectListItem> CategoryList { get; set; }
    }

}
