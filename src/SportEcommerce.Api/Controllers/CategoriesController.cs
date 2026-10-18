using Microsoft.AspNetCore.Mvc;
using SportEcommerce.Application.Categories;

namespace SportEcommerce.Api.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(ICategoryService categories) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CategoryDto>> List(CancellationToken cancellationToken) =>
        categories.ListAsync(cancellationToken);
}
