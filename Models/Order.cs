using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GazeManager.Models
{
    public class Order
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public User User { get; set; }

        public string Code { get; set; }

        public int Status { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Address { get; set; }

        public int ShippingOption { get; set; }

        public DateTime ShipDate { get; set; }

        [Column(TypeName = "DECIMAL(18, 2)")]
        public decimal ShippingFee { get; set; }

        public string Phone { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedDate { get; set; }

        public List<ProductOrder> ProductOrders { get; set; } = new List<ProductOrder>();

    }

    public enum OrderStatus
    {
        Submitted = 0,
        Delivering = 1,
        Completed = 2,
        Cancelled = 3
    }
}