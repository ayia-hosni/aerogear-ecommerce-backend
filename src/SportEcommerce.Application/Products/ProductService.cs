using Microsoft.EntityFrameworkCore;
using SportEcommerce.Application.Abstractions;
using SportEcommerce.Application.Common;
using SportEcommerce.Domain.Entities;

namespace SportEcommerce.Application.Products;

public interface IProductService
{
    Task<PagedResult<ProductDto>> SearchAsync(ProductQuery query, CancellationToken cancellationToken);
    Task<ProductDto> GetBySlugAsync(string slug, CancellationToken cancellationToken);
}

internal sealed class ProductService(IAppDbContext db) : IProductService
{
    public async Task<PagedResult<ProductDto>> SearchAsync(ProductQuery query, CancellationToken cancellationToken)
    {
        var products = db.Products.AsNoTracking().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            products = products.Where(p => p.Category.Slug == query.Category);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            products = products.Where(p =>
                EF.Functions.Like(p.Name, $"%{term}%") ||
                EF.Functions.Like(p.Summary, $"%{term}%"));
        }

        if (query.Featured is { } featured)
        {
            products = products.Where(p => p.IsFeatured == featured);
        }

        var totalCount = await products.CountAsync(cancellationToken);

        var items = await Sort(products, query.Sort)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ProductMapping.ToDto)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDto>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<ProductDto> GetBySlugAsync(string slug, CancellationToken cancellationToken) =>
        await db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.Slug == slug)
            .Select(ProductMapping.ToDto)
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(nameof(Product), slug);

    private static IQueryable<Product> Sort(IQueryable<Product> products, ProductSort sort) => sort switch
    {
        ProductSort.PriceAscending => products.OrderBy(p => p.Price).ThenBy(p => p.Id),
        ProductSort.PriceDescending => products.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
        ProductSort.TopRated => products.OrderByDescending(p => p.Rating).ThenByDescending(p => p.ReviewCount).ThenBy(p => p.Id),
        _ => products.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.Id)
    };
}
