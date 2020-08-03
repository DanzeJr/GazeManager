using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Bogus;
using Newtonsoft.Json;

namespace GazeManager.Models
{
    public class ProductOption
    {
        public long Id { get; set; }

        public long ProductId { get; set; }

        public Product Product { get; set; }

        public int Size { get; set; }

        public string Color { get; set; }

        public string Image { get; set; }

        [Column(TypeName = "DECIMAL(18, 2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "DECIMAL(18, 2)")]
        public decimal Discount { get; set; }

        public long Stock { get; set; }

        public int Status { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime LastUpdate { get; set; } = DateTime.Now;

        public static readonly Faker<ProductOption> Faker = new Faker<ProductOption>().StrictMode(false)
            .RuleFor(x => x.Status, f => f.Random.Number(0, 2))
            .RuleFor(x => x.Color, f => f.Commerce.Color().ToUpperInvariant())
            .RuleFor(x => x.Size, f => f.Random.Number(0, 2))
            .RuleFor(x => x.Image, f => f.Image.LoremFlickrUrl(keywords: "sunglass, eyeglass", matchAllKeywords: true, width: 1000, height: 1000))
            .RuleFor(x => x.Price, f => f.Random.Decimal(2, 100))
            .RuleFor(x => x.Discount, f => f.Random.Decimal(2, 100))
            .RuleFor(x => x.Stock, (f, c) => c.Status != 0 ? 0 : f.Random.Number(1, 300))
            .RuleFor(x => x.LastUpdate, f => f.Date.Recent(15))
            .RuleFor(x => x.CreatedDate, (f, c) => f.Date.Recent(150, c.LastUpdate));
    }
}