using System;

namespace GazeManager.Models
{
    public class Cart
    {
        public long Id { get; set; }

        public long ProductId { get; set; }

        public Product Product { get; set; }

        public long UserId { get; set; }

        public User User { get; set; }

        public int Quantity { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}