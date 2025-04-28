using System.Net.Mail;
using Divergent.Api.Domain;
using Divergent.Data;
using Divergent.Data.Models;
using MediatR;

namespace Divergent.Api.Features.Email;

internal sealed class OrderCreatedHandler(DivergentDbContext db) : INotificationHandler<OrderCreated>
{
    public async Task Handle(OrderCreated notification, CancellationToken cancellationToken)
    {
        var orderCollection = db.Database.GetCollection<Order>();
        var order = orderCollection.FindById(notification.OrderId);

        var customerCollection = db.Database.GetCollection<Customer>();
        var customer = customerCollection.FindById(order.CustomerId);

        var productCollection = db.Database.GetCollection<Product>();
        var products = productCollection.Query().Where(p => order.Items.Contains(p.Id)).ToList();

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