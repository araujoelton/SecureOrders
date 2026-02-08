using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using SecureOrders.Domain.Orders;

namespace SecureOrders.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Order> Orders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
