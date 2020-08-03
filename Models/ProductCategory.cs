using System.Text.Json.Serialization;

namespace GazeManager.Models
{
    public class ProductCategory
    {
        public long Id { get; set; }

        public long ProductId { get; set; }

        [JsonIgnore]
        public virtual Product Product { get; set; }

        public long CategoryId { get; set; }

        [JsonIgnore]
        public virtual Category Category { get; set; }
    }
}