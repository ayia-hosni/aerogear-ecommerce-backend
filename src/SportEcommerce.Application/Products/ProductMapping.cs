using System.Linq.Expressions;
using SportEcommerce.Domain.Entities;

namespace SportEcommerce.Application.Products;

internal static class ProductMapping
{
    public static readonly Expression<Func<Product, ProductDto>> ToDto = p => new ProductDto(
        p.Id,
        p.Name,
        p.Slug,
        p.Series,
        p.Summary,
        p.Badge,
        p.Price,
        p.Stock,
        p.Rating,
        p.ReviewCount,
        p.ImageUrl,
        p.Gallery,
        p.Tags,
        p.Sizes,
        p.SoldOutSizes,
        p.Colors,
        p.Theme,
        p.IsFeatured,
        p.Category.Slug,
        p.Category.Name);
}
