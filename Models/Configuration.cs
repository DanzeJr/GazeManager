using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GazeManager.Models
{
    public class Configuration
    {
        public long Id { get; set; }

        public string MaxVersion { get; set; }

        public string MinVersion { get; set; }

        public int MinStandardDeliveryDays { get; set; }

        public int MinPremiumDeliveryDays { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}