namespace OnlineBookManagementSystem.Models.ViewModel
{
    public class ShoppingCartViewModel
    {
        public int Id { get; internal set; }
        public int BookId { get; internal set; }
        public string BookTitle { get; internal set; }
        public string? BookAuthor { get; internal set; }
        public decimal BookPrice { get; internal set; }
        public string? BookImage { get; internal set; }
        public int Quantity { get; internal set; }
        public decimal Subtotal { get; internal set; }
        public string CategoryName { get; internal set; }
    }
}