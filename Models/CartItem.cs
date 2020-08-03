using System;
using Newtonsoft.Json;

namespace GazeManager.Models
{
    public class CartItem
    {
        public long Id { get; set; }

        public long OptionId { get; set; }

        public virtual ProductOption Option { get; set; }

        public long UserId { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        public int Quantity { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}