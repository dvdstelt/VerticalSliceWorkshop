using Configuration;
using Divergent.Data;
using Divergent.Data.Models;
using MediatR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

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
    public Task<IEnumerable<OrderViewModel>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        // Load orders
        var orderCollection = db.Database.GetCollection<Order>();
        var orders = orderCollection.Query().Limit(10).ToList();

        // Load customers
        var customerIds = orders.Select(o => o.CustomerId).Distinct().ToArray();
        var customerCollection = db.Database.GetCollection<Customer>();
        var customers = customerCollection.Query().Where(c => customerIds.Contains(c.Id)).ToList();

        // Load products
        var productIds = orders.SelectMany(o => o.Items).Distinct().ToArray();
        var productCollection = db.Database.GetCollection<Product>();
        var products = productCollection.Query().Where(p => productIds.Contains(p.Id)).ToList();

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

        return Task.FromResult(orderViewModels);
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