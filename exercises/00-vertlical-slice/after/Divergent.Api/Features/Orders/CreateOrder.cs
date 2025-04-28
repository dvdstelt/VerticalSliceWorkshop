using Divergent.Api.Domain;
using Divergent.Data;
using Divergent.Data.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Divergent.Api.Features.Orders;

[ApiController]
public class CreateOrder(IMediator mediator) : Controller
{
    [HttpPost("/api/orders")]
    public async Task<IActionResult> Post(OrderViewModel order)
    {
        var orderId = await mediator.Send(new CreateOrderCommand(order.CustomerId, order.Products.Select(p => p.ProductId).ToList()));

        return Ok(orderId);
    }

    public class OrderViewModel
    {
        public int CustomerId { get; set; }
        public List<ProductViewModel> Products { get; set; }
    }

    public class ProductViewModel
    {
        public int ProductId { get; set; }
    }
}

public record CreateOrderCommand(int CustomerId, List<int> Products) : IRequest<int>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(100);
        RuleFor(x => x.Products).NotEmpty();
        RuleForEach(x => x.Products).GreaterThan(100);
        RuleFor(x => x.Products).Must(x => x.Distinct().Count() == x.Count);
    }
}

internal sealed class CreateOrderHandler(DivergentDbContext db, IPublisher publisher) : IRequestHandler<CreateOrderCommand, int>
{
    public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order()
        {
            CustomerId = request.CustomerId,
            Items = request.Products
        };

        var orderCollection = db.Database.GetCollection<Order>();
        order.Id = orderCollection.Insert(order);

        await publisher.Publish(new OrderCreated(order.Id), cancellationToken);

        return order.Id;
    }
}
