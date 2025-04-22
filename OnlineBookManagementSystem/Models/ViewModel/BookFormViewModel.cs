using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class BookFormViewModel
    {
        public Book Book { get; set; }

        public IEnumerable<SelectListItem> CategoryList { get; set; }
    }

}
