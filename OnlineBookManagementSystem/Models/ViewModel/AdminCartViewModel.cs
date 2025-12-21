
namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class AdminCartViewModel
    {
        public int Id { get; internal set; }
        public string UserName { get; internal set; }
        public string BookTitle { get; internal set; }
        public decimal Subtotal { get; internal set; }
        public DateTime AddedAt { get; internal set; }
        public int Quantity { get; internal set; }
        public string? UserEmail { get; internal set; }
    }
}