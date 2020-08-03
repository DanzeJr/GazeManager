namespace GazeManager.Models
{
    public class ProductImage
    {
        public long Id { get; set; }

        public long ProductId { get; set; }

        public virtual Product Product { get; set; }

        public string Name { get; set; }

    }
}