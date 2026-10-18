using Microsoft.AspNetCore.Mvc;
using SportEcommerce.Application.Common;
using SportEcommerce.Application.Products;

namespace SportEcommerce.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IProductService products) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<ProductDto>> Search([FromQuery] ProductQuery query, CancellationToken cancellationToken) =>
        products.SearchAsync(query, cancellationToken);

    [HttpGet("{slug}")]
    public Task<ProductDto> GetBySlug(string slug, CancellationToken cancellationToken) =>
        products.GetBySlugAsync(slug, cancellationToken);
}
