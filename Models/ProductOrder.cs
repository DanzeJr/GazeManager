namespace GazeManager.Models
{
    public class ProductOrder
    {
        public long Id { get; set; }

        public long ProductId { get; set; } = -1;

        public Product Product { get; set; }

        public long OrderId { get; set; } = -1;

        public Order Order { get; set; }

        public int Quantity { get; set; }
    }
}