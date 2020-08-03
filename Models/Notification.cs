using System;
using Newtonsoft.Json;

namespace GazeManager.Models
{
    public class Notification
    {
        public long Id { get; set; }

        public long? UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public int Type { get; set; }

        public long ObjectId { get; set; }

        public string Image { get; set; }

        public int Status { get; set; }

        public DateTime CreatedDate { get; set; }
    }

    public enum NotificationType
    {
        News,
        Order,
        Product
    }
}