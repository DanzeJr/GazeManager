using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Bogus;

namespace GazeManager.Models
{
    public class Product
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public int Size { get; set; }

        public string Image { get; set; }

        [Column(TypeName = "DECIMAL(18, 2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "DECIMAL(18, 2)")]
        public decimal Discount { get; set; }

        public long Stock { get; set; }

        public string Description { get; set; }

        public int Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdate { get; set; }

        public List<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

        public List<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

        public static readonly Faker<Product> Faker = new Faker<Product>().StrictMode(false)
            .RuleFor(x => x.Color, f => f.Commerce.Color().ToUpperInvariant())
            .RuleFor(x => x.Size, f => f.Random.Number(0, 2))
            .RuleFor(x => x.Status, f => f.Random.Bool(0.8f) ? 0 : f.Random.Number(1, 2))
            .RuleFor(x => x.Description, f => f.Lorem.Sentence())
            .RuleFor(x => x.Image, f => f.Image.LoremFlickrUrl(keywords: "sunglass, eyeglass", matchAllKeywords: true, width: 1000, height: 1000))
            .RuleFor(x => x.Price, f => f.Random.Decimal(2, 100))
            .RuleFor(x => x.Discount, f => f.Random.Decimal(2, 100))
            .RuleFor(x => x.Stock, (f, c) => c.Status != 0 ? 0 : f.Random.Number(1, 300))
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