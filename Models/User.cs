using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GazeManager.Models
{
    public class User
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string Avatar { get; set; }

        [NotMapped]
        public string Password { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string Role { get; set; }

        public virtual List<CartItem> Cart { get; set; } = new List<CartItem>();

        public virtual List<Order> Orders { get; set; } = new List<Order>();

        public virtual List<DeviceInfo> Devices { get; set; } = new List<DeviceInfo>();
    }

    public class Role
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";
    }
}