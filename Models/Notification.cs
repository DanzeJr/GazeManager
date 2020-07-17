using System;

namespace GazeManager.Models
{
    public class Notification
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string Type { get; set; }

        public long ObjectId { get; set; }

        public string Image { get; set; }

        public string Code { get; set; }

        public string Status { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}