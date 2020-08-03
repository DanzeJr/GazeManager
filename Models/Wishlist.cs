using System;
using Newtonsoft.Json;

namespace GazeManager.Models
{
    public class Wishlist
    {
        public long Id { get; set; }

        public long ProductId { get; set; }

        public Product Product { get; set; }

        public long UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}