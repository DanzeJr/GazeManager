using System;

namespace GazeManager.Models
{
    public class NewsInfo
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string BriefContent { get; set; }

        public string FullContent { get; set; }

        public string Image { get; set; }

        public string Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdate { get; set; }
    }
}