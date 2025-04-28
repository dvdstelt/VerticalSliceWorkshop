using Divergent.Api.Features.Orders;
using Divergent.Data;
using Divergent.Data.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
    public Task<OrderViewModel> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        // Load order
        var orderCollection = db.Database.GetCollection<Order>();
        var order = orderCollection.Query().Where(o => o.Id == request.OrderId).FirstOrDefault();

        // Load customer
        var customerCollection = db.Database.GetCollection<Customer>();
        var customer = customerCollection.Query().Where(c => c.Id == order.CustomerId).FirstOrDefault();

        // Load products
        var productCollection = db.Database.GetCollection<Product>();
        var products = productCollection.Query().Where(p => order.Items.Contains(p.Id)).ToList();

        // Map to ViewModel
        var orderViewModel = new OrderViewModel
        {
            OrderId = order.Id,
            Customer = customer != null ? new CustomerViewModel { Id = customer.Id, Name = customer.Name } : null,
            Products = products.Select(p => new ProductViewModel { Id = p.Id, Name = p.Name, Price = p.Price }),
            TotalPrice = products.Sum(p => p.Price)
        };

        return Task.FromResult(orderViewModel);
    }
}

public record GetOrderDetailsQuery(int OrderId) : IRequest<OrderViewModel>;
