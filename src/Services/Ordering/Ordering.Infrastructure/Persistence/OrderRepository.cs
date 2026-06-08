using Microsoft.EntityFrameworkCore;
using Ordering.Application.Orders;
using Ordering.Domain.Orders;

namespace Ordering.Infrastructure.Persistence;

public sealed class OrderRepository(OrderingDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        await context.Orders.AddAsync(order, cancellationToken);
    }

    public void Update(Order order)
    {
        context.Orders.Update(order);
    }
}
