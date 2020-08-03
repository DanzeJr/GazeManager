using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GazeManager.Models
{
    public class Configuration
    {
        public long Id { get; set; }

        public string MaxVersion { get; set; }

        public string MinVersion { get; set; }

        public int MinPasswordLength { get; set; }

        public int MinStandardDeliveryDays { get; set; }

        [Column(TypeName = "DECIMAL(18, 2)")]
        public decimal StandardShippingFee { get; set; }

        public int MinPremiumDeliveryDays { get; set; }

        [Column(TypeName = "DECIMAL(18, 2)")]
        public decimal PremiumShippingFee { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}