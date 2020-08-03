using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Bogus;
using Newtonsoft.Json;

namespace GazeManager.Models
{
    public class Category
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Icon { get; set; }

        public string Brief { get; set; }

        public string Color { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [JsonIgnore]
        public List<ProductCategory> ProductCategories { get; set; }

        [NotMapped]
        public List<Product> Products
        {
            get { return ProductCategories?.Select(x =>
            {
                x.Product.ProductCategories = null;
                return x.Product;
            }).ToList(); }
        }

        public static readonly Faker<Category> Faker = new Faker<Category>().StrictMode(false)
            .RuleFor(x => x.Color, f => f.Internet.Color())
            .RuleFor(x => x.Brief, f => f.Lorem.Paragraph(1))
            .RuleFor(x => x.Icon, f => f.Image.LoremFlickrUrl(keywords: "category", matchAllKeywords: true, width: 1000, height: 1000))
            .RuleFor(x => x.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(x => x.CreatedDate, f => f.Date.Recent(15));
    }
}