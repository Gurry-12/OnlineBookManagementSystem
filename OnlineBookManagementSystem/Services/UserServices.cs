using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;

namespace OnlineBookManagementSystem.Services
{
    public class UserServices : IUserInterface
    {
        private readonly BookManagementContext _context;
        public UserServices(BookManagementContext context) 
        {
            _context = context;
        }


    }
}
