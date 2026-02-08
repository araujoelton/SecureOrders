using Microsoft.EntityFrameworkCore;
using SecureOrders.Application.Common.Interfaces;
using SecureOrders.Domain.Orders;

namespace SecureOrders.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(b =>
        {
            b.ToTable("Orders");
            b.HasKey(x => x.Id);
            b.Property(x => x.CustomerId).HasMaxLength(100).IsRequired();
            b.Property(x => x.TotalAmount).HasPrecision(18, 2).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
