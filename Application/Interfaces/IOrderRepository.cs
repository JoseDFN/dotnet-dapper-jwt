using Domain.Entities;

namespace Application.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    // Crea una orden con items (Order + OrderItems)
    Task<int> CreateOrderAsync(Order order, IEnumerable<OrderItem> items);

    // Obtiene detalles de una orden con items
    Task<Order?> GetOrderWithItemsAsync(int orderId);

    // Lista todas las órdenes de un usuario
    Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId);
}
