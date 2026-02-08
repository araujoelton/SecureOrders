using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureOrders.Domain.Orders
{
    public sealed class Order
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string CustomerId { get; private set; } = default!;
        public decimal TotalAmount { get; private set; }
        public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

        private Order() { } //EF

        public Order(string customerId, decimal totalAmount)
        {
            if(string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("CustomerId cannot be null or empty.", nameof(customerId));

            if(totalAmount < 0)
                throw new ArgumentOutOfRangeException(nameof(totalAmount), "TotalAmount cannot be negative.");

            CustomerId = customerId;
            TotalAmount = totalAmount;
        }
    }
}
