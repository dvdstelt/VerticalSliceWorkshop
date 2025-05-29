using Divergent.Data;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Divergent.Api.Features.Orders;

public class GetOrders(IMediator mediator) : Controller
{
    [HttpGet("/api/orders")]
    public async Task<IActionResult> Get()
    {
        var result = await mediator.Send(new GetOrdersQuery());

        return Ok(result);
    }
}

internal sealed class GetMyOrdersHandler(DivergentDbContext db) : IRequestHandler<GetOrdersQuery, IEnumerable<OrderViewModel>>
{
    public async Task<IEnumerable<OrderViewModel>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        // Load orders
        var orders = await db.Orders
            .OrderByDescending(o => o.DateTimeUtc)
            .Take(10)
            .ToListAsync(cancellationToken);

        // Load customers
        var customerIds = orders.Select(o => o.CustomerId).Distinct().ToArray();
        var customers = await db.Customers
            .Where(c => customerIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        // Load products
        var productIds = orders.SelectMany(o => o.Items).Distinct().ToArray();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        // Map everything to ViewModels
        var orderViewModels = orders.Select(o => new OrderViewModel
        {
            OrderId = o.Id,
            DateTimeUtc = o.DateTimeUtc,
            State = o.State,
            Customer = customers.Where(c => c.Id == o.CustomerId).Select(c => new CustomerViewModel { Id = c.Id, Name = c.Name }).FirstOrDefault(),
            Products = products.Where(p => o.Items.Contains(p.Id))
                .Select(p => new ProductViewModel { Id = p.Id, Name = p.Name, Price = p.Price }),
            ItemsCount = o.Items.Count(),
            TotalPrice = products.Where(p => o.Items.Contains(p.Id)).Sum(p => p.Price)
        });

        return orderViewModels;
    }
}

public record GetOrdersQuery : IRequest<IEnumerable<OrderViewModel>>;

public class OrderViewModel
{
    public int OrderId { get; set; }
    public CustomerViewModel Customer { get; set; }
    public DateTime DateTimeUtc { get; set; }
    public string State { get; set; }
    public IEnumerable<ProductViewModel> Products { get; set; }
    public int ItemsCount { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CustomerViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class ProductViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

