using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportEcommerce.Api.Extensions;
using SportEcommerce.Application.Orders;

namespace SportEcommerce.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrdersController(IOrderService orders) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<OrderDto>> List(CancellationToken cancellationToken) =>
        orders.ListAsync(User.GetUserId(), cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<OrderDto> Get(Guid id, CancellationToken cancellationToken) =>
        orders.GetAsync(User.GetUserId(), id, cancellationToken);

    [HttpPost]
    [ProducesResponseType<OrderDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await orders.CreateAsync(User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }
}
