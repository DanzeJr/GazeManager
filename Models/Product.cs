using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Bogus;
using Newtonsoft.Json;

namespace GazeManager.Models
{
    public class Product
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        public int Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdate { get; set; }

        [JsonIgnore]
        public List<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

        [NotMapped]
        public List<Category> Categories => ProductCategories?.Select(x => x.Category).ToList();

        [JsonIgnore]
        public List<ProductOption> ProductOptions { get; set; } = new List<ProductOption>();

        [NotMapped]
        public List<ProductOption> Options
        {
            get
            {
                return ProductOptions?.Select(x =>
                {
                    x.Product = null;
                    return x;
                }).ToList();
            }
        }

        [JsonIgnore]
        public virtual List<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

        [NotMapped]
        public List<string> Images
        {
            get { return ProductImages?.Select(x => x.Name).ToList(); }
        }

        public static readonly Faker<Product> Faker = new Faker<Product>().StrictMode(false)
            .RuleFor(x => x.Status, f => f.Random.Bool(0.8f) ? 0 : f.Random.Number(1, 2))
            .RuleFor(x => x.Image, f => f.Image.LoremFlickrUrl(keywords: "sunglass, eyeglass", matchAllKeywords: true, width: 1000, height: 1000))
            .RuleFor(x => x.Description, f => f.Lorem.Sentence())
            .RuleFor(x => x.LastUpdate, f => f.Date.Recent(15))
            .RuleFor(x => x.CreatedDate, (f, c) => f.Date.Recent(150, c.LastUpdate))
            .RuleFor(x => x.Name, f => f.Random.Bool() ? f.Commerce.Product() : f.Commerce.ProductName());
    }

    public enum ProductStatus
    {
        ReadyStock = 0,
        OutOfStock = 1,
        Suspend = 2
    }

    public enum ProductSize
    {
        S = 0,
        M = 1,
        L = 2
    }
}