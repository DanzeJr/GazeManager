using Bogus;
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

        public static readonly Faker<NewsInfo> Faker = new Faker<NewsInfo>().StrictMode(false)
            .RuleFor(c => c.BriefContent, f => f.Lorem.Sentence())
            .RuleFor(c => c.FullContent, f => f.Lorem.Paragraph())
            .RuleFor(c => c.CreatedDate, f => f.Date.Recent(7))
            .RuleFor(c => c.Title, f => f.Lorem.Sentence(10))
            .RuleFor(c => c.Image,
                f => f.Image.LoremFlickrUrl(keywords: "sunglass, eyeglass", matchAllKeywords: true, width: 1980,
                    height: 1080))
            .RuleFor(c => c.LastUpdate, f => f.Date.Recent(4));
    }
}