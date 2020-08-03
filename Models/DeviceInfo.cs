using System;

namespace GazeManager.Models
{
    public class DeviceInfo
    {
        public long Id { get; set; }

        public string Device { get; set; }

        public string OsVersion { get; set; }

        public string AppVersion { get; set; }

        public string InstanceId { get; set; }

        public string RegId { get; set; }

        public long? UserId { get; set; }

        public User User { get; set; }

        public int Status { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime LastUpdate { get; set; } = DateTime.Now;
    }
}