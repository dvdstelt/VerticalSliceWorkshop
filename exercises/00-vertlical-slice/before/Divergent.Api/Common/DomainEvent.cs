using MediatR;

namespace Divergent.Api.Common;

public abstract class DomainEvent : INotification
{
    public DateTimeOffset DateOccurred { get; protected set; } = DateTimeOffset.UtcNow;
}
