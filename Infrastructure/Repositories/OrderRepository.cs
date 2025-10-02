using Application.DTOs.Orders;
using Application.Interfaces;
using Dapper;
using Domain.Entities;
using Newtonsoft.Json;
using System.Data;

namespace Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(IDbConnection connection, IDbTransaction transaction)
        : base(connection, transaction) { }
    
    public async Task<int> CreateOrderUsingFunctionAsync(int userId, IEnumerable<CreateOrderItemDto> items)
    {
        var jsonItems = JsonConvert.SerializeObject(items);

        var sql = "SELECT create_order(@UserId, @Items::json)";
        var orderId = await _connection.ExecuteScalarAsync<int>(
            sql,
            new { UserId = userId, Items = jsonItems },
            _transaction
        );

        return orderId;
    }

    public override async Task<int> AddAsync(Order order)
    {
        var sql = @"INSERT INTO orders (user_id, total, created_at, updated_at)
                    VALUES (@UserId, @Total, @Created_At, @Updated_At)
                    RETURNING id;";
        var id = await _connection.ExecuteScalarAsync<int>(sql, order, _transaction);
        order.Id = id;
        return id;
    }

    public override async Task UpdateAsync(Order order)
    {
        var sql = @"UPDATE orders
                    SET total=@Total, updated_at=@Updated_At
                    WHERE id=@Id";
        await _connection.ExecuteAsync(sql, order, _transaction);
    }

    public async Task<int> CreateOrderAsync(Order order, IEnumerable<OrderItem> items)
    {
        var orderId = await AddAsync(order);

        foreach (var item in items)
        {
            item.OrderId = orderId;
            var sqlItem = @"INSERT INTO order_items (order_id, product_id, quantity, unit_price, created_at, updated_at)
                            VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @Created_At, @Updated_At)";
            await _connection.ExecuteAsync(sqlItem, item, _transaction);
        }

        return orderId;
    }

    public async Task<Order?> GetOrderWithItemsAsync(int orderId)
    {
        var sql = @"SELECT o.id, o.user_id as UserId, o.total, o.created_at, o.updated_at,
                           oi.id, oi.order_id, oi.product_id, oi.quantity, oi.unit_price, oi.created_at, oi.updated_at,
                           p.id, p.name, p.sku, p.price, p.stock, p.category, p.created_at, p.updated_at
                    FROM orders o
                    INNER JOIN order_items oi ON o.id = oi.order_id
                    INNER JOIN products p ON oi.product_id = p.id
                    WHERE o.id=@OrderId";

        var orderDict = new Dictionary<int, Order>();

        var result = await _connection.QueryAsync<Order, OrderItem, Product, Order>(
            sql,
            (o, oi, p) =>
            {
                if (!orderDict.TryGetValue(o.Id, out var currentOrder))
                {
                    currentOrder = o;
                    currentOrder.OrderItems = new List<OrderItem>();
                    orderDict.Add(currentOrder.Id, currentOrder);
                }
                oi.Product = p;
                currentOrder.OrderItems!.Add(oi);
                return currentOrder;
            },
            new { OrderId = orderId },
            _transaction,
            splitOn: "id,id" // importante para mapear bien
        );

        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId)
    {
        var sql = "SELECT * FROM orders WHERE user_id=@UserId";
        return await _connection.QueryAsync<Order>(sql, new { UserId = userId }, _transaction);
    }
}