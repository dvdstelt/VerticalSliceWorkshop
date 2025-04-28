using Divergent.Api.Common;

namespace Divergent.Api.Domain;

public class OrderCreated(int orderId) : DomainEvent
{
    public int OrderId { get; } = orderId;
}