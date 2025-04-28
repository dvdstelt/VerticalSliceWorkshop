using Divergent.Data.Models;

namespace Divergent.Data.Migrations;

internal static class SeedData
{
    internal static List<Product> Products()
    {
        return
        [
            new Product { Id = 1, Name = "Star Wars : The Phantom Menace", Price = 5 },
            new Product { Id = 2, Name = "Star Wars : Attack of the Clones", Price = 4 },
            new Product { Id = 3, Name = "Star Wars : Revenge of the Sith", Price = 7 },
            new Product { Id = 4, Name = "Star Wars : A New Hope", Price = 20 },
            new Product { Id = 5, Name = "Star Wars : The Empire Strikes Back", Price = 30 },
            new Product { Id = 6, Name = "Star Wars : Return of the Jedi", Price = 25 }
        ];
    }

    internal static List<Customer> Customers()
    {
        return
        [
            new Customer { Id = 1, Name = "Nimbus Ventures", Description = "Cloudy with a chance of scaling.", Email = "contact@nimbusventures.com" },
            new Customer { Id = 2, Name = "BitCrate Logistics", Description = "We ship everything, including bugs.", Email = "sales@bitcrate.com"},
            new Customer
            {
                Id = 3, Name = "Quantum Pickle Co.", Description = "No idea what we do, but we're disrupting something.", Email = "no-reply@quantumpickle.com"
            }
        ];
    }

    internal static List<Order> Orders()
    {
        return
        [
            new Order
            {
                CustomerId = 1, DateTimeUtc = new DateTime(2025, 01, 01), State = "Payment Succeeded",
                Id = 1, Items = [1, 2]
            },
            new Order
            {
                CustomerId = 3, DateTimeUtc = new DateTime(2025, 01, 02), State = "Payment awaiting",
                Id = 2, Items = [4, 5, 6]
            }
        ];
    }

}