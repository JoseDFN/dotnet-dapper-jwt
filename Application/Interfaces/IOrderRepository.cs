using Application.DTOs.Orders;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        // Crea una orden con items (Order + OrderItems) — método viejo
        Task<int> CreateOrderAsync(Order order, IEnumerable<OrderItem> items);

        // Nueva forma: llama directamente a la función en Postgres
        Task<int> CreateOrderUsingFunctionAsync(int userId, IEnumerable<CreateOrderItemDto> items);

        // Obtiene detalles de una orden con items
        Task<Order?> GetOrderWithItemsAsync(int orderId);

        // Lista todas las órdenes de un usuario
        Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId);
    }
}