using System;

namespace GazeManager.Models
{
    public class Wishlist
    {
        public long ProductId { get; set; }

        public string Name { get; set; }

        public string Image { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}