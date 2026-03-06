using System.Net.Mail;
using Divergent.Api.Domain;
using Divergent.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Divergent.Api.Features.Email;

internal sealed class OrderCreatedHandler(DivergentDbContext db) : INotificationHandler<OrderCreated>
{
    public async Task Handle(OrderCreated notification, CancellationToken cancellationToken)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == notification.OrderId, cancellationToken);
        if (order == null) return;

        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == order.CustomerId, cancellationToken);
        if (customer == null) return;

        var products = await db.Products.Where(p => order.Items.Contains(p.Id)).ToListAsync(cancellationToken);

        var mailMessage = new MailMessage
        {
            From = new MailAddress("orders@divergent.com"),
            To = { new MailAddress(customer.Email) },
            Subject = $"Order #{order.Id} Confirmation",
            Body = $"""
                Dear {customer.Name},

                Thank you for your order #{order.Id}.

                Order details:
                {string.Join(Environment.NewLine, products.Select(p => $"- {p.Name}: ${p.Price}"))}

                Total: ${products.Sum(p => p.Price)}

                Best regards,
                Divergent Team
                """
        };

        var smtpClient = new SmtpClient("localhost", 25);
        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
    }
}

