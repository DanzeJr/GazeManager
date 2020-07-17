using System;
using System.Collections.Generic;

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

        public DateTime CreatedDate { get; set; }

        public string Role { get; set; }

        public List<Cart> CartList { get; set; } = new List<Cart>();

        public List<Order> Orders { get; set; } = new List<Order>();

        public List<DeviceInfo> DeviceInfos { get; set; } = new List<DeviceInfo>();
    }

    public class Role
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";
    }
}