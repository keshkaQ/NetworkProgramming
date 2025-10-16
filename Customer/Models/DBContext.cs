using Microsoft.EntityFrameworkCore;

namespace Consumer.Models;

public class DBContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=iphoneStore.db");
    }
}