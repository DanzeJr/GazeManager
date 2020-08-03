using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace GazeManager.Models
{
    public class ProductOrder
    {
        public long Id { get; set; }

        public long OptionId { get; set; } = -1;

        public ProductOption Option { get; set; }

        public long OrderId { get; set; } = -1;

        [JsonIgnore]
        public Order Order { get; set; }

        public string Color { get; set; }

        public int Size { get; set; }

        [Column(TypeName = "DECIMAL(18, 2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "DECIMAL(18, 2)")]
        public decimal Discount { get; set; }

        public int Quantity { get; set; }
    }
}