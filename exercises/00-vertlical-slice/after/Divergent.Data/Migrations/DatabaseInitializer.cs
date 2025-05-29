namespace Divergent.Data.Migrations;

public static class DatabaseInitializer
{
    public static void Initialize(DivergentDbContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        // Ensure database and tables are created
        context.Database.EnsureCreated();

        // Seed Customers
        if (!context.Customers.Any())
        {
            context.Customers.AddRange(SeedData.Customers());
        }

        // Seed Orders
        if (!context.Orders.Any())
        {
            context.Orders.AddRange(SeedData.Orders());
        }

        // Seed Products
        if (!context.Products.Any())
        {
            context.Products.AddRange(SeedData.Products());
        }

        context.SaveChanges();
    }
}

