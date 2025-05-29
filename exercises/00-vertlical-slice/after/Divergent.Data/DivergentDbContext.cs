using Microsoft.EntityFrameworkCore;

namespace Divergent.Data;

public class DivergentDbContext : DbContext
{
    public DivergentDbContext(DbContextOptions<DivergentDbContext> options) : base(options)
    {
    }

    public DbSet<Models.Order> Orders { get; set; }
    public DbSet<Models.Customer> Customers { get; set; }
    public DbSet<Models.Product> Products { get; set; }

    public static string GetDatabasePath(string databaseName)
    {
        var storagePath = Path.Combine(FindStoragePath(), ".db");
        Directory.CreateDirectory(storagePath);
        return Path.Combine(storagePath, databaseName.ToLower() + ".db");
    }

    static string FindStoragePath()
    {
        var directory = AppDomain.CurrentDomain.BaseDirectory;
        while (true)
        {
            if (Directory.EnumerateFiles(directory).Any(file => file.EndsWith(".sln")))
            {
                return directory;
            }

            // When no solution file was found, try to find a database directory
            var databaseDirectory = Path.Combine(directory, DefaultDatabaseDirectory);
            if (Directory.Exists(databaseDirectory))
            {
                return databaseDirectory;
            }

            var parent = Directory.GetParent(directory);
            if (parent == null)
                throw new DirectoryNotFoundException("Solution folder not found.");
            directory = parent.FullName;
        }
    }

    // If you have additional configuration, add it here
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Add custom model configuration if needed
    }
    
    const string DefaultDatabaseDirectory = ".db";
}