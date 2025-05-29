using Divergent.Api.Features.Orders;
using Divergent.Data;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Divergent.Api.Features.OrderDetails;

public class GetOrderDetails(IMediator mediator) : Controller
{
    [HttpGet("/api/order/{orderId}")]
    public async Task<IActionResult> Get([FromRoute] int orderId)
    {
        var result = await mediator.Send(new GetOrderDetailsQuery(orderId));

        return Ok(result);
    }
}

internal sealed class GetOrderDetailsHandler(DivergentDbContext db) : IRequestHandler<GetOrderDetailsQuery, OrderViewModel>
{
    public async Task<OrderViewModel> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        // Load order
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
        if (order == null)
            return null;

        // Load customer
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == order.CustomerId, cancellationToken);

        // Load products
        var products = await db.Products.Where(p => order.Items.Contains(p.Id)).ToListAsync(cancellationToken);

        // Map to ViewModel
        var orderViewModel = new OrderViewModel
        {
            OrderId = order.Id,
            Customer = customer != null ? new CustomerViewModel { Id = customer.Id, Name = customer.Name } : null,
            Products = products.Select(p => new ProductViewModel { Id = p.Id, Name = p.Name, Price = p.Price }),
            TotalPrice = products.Sum(p => p.Price)
        };

        return orderViewModel;
    }
}

public record GetOrderDetailsQuery(int OrderId) : IRequest<OrderViewModel>;
