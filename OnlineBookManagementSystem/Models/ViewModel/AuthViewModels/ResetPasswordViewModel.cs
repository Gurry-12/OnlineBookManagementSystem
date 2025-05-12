using System.ComponentModel.DataAnnotations;

namespace OnlineBookManagementSystem.Models.ViewModel.AuthViewModels
{
    public class ResetPasswordViewModel
    {
        public string Email { get; set; }

        public string NewPassword { get; set; }
    }
}
