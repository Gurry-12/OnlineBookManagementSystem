using OnlineBookManagementSystem.Models;

namespace OnlineBookManagementSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly BookManagementContext _context;

        public OrderService(BookManagementContext context)
        {
            _context = context;
        }

    }

}
