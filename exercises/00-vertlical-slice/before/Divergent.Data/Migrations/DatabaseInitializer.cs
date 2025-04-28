using Divergent.Data.Models;
using LiteDB;

namespace Divergent.Data.Migrations;

public static class DatabaseInitializer
{
    public static void Initialize(LiteDatabase context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var customers = context.GetCollection<Customer>();
        if (customers.Count() > 0)
            return;

        customers.InsertBulk(SeedData.Customers());

        var orders = context.GetCollection<Order>();
        orders.InsertBulk(SeedData.Orders());

        var products = context.GetCollection<Product>();
        products.InsertBulk(SeedData.Products());
    }
}