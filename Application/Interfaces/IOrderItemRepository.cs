using Domain.Entities;

namespace Application.Interfaces;

public interface IOrderItemRepository : IGenericRepository<OrderItem>
{
    // Si necesitas consultas específicas de items
    Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId);
}