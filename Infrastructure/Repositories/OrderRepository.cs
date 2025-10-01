using Application.Interfaces;
using Dapper;
using Domain.Entities;
using System.Data;

namespace Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(IDbConnection connection, IDbTransaction transaction)
        : base(connection, transaction) { }

    public override async Task<int> AddAsync(Order order)
    {
        var sql = @"INSERT INTO orders (userid, total, createdat, updatedat)
                    VALUES (@UserId, @Total, @CreatedAt, @UpdatedAt)
                    RETURNING id;";
        var id = await _connection.ExecuteScalarAsync<int>(sql, order, _transaction);
        order.Id = id;
        return id;
    }

    public override async Task UpdateAsync(Order order)
    {
        var sql = @"UPDATE orders
                    SET total=@Total, updatedat=@UpdatedAt
                    WHERE id=@Id";
        await _connection.ExecuteAsync(sql, order, _transaction);
    }

    public async Task<int> CreateOrderAsync(Order order, IEnumerable<OrderItem> items)
    {
        var orderId = await AddAsync(order);

        foreach (var item in items)
        {
            item.OrderId = orderId;
            var sqlItem = @"INSERT INTO orderitems (orderid, productid, quantity, unitprice, createdat, updatedat)
                            VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @CreatedAt, @UpdatedAt)";
            await _connection.ExecuteAsync(sqlItem, item, _transaction);
        }

        return orderId;
    }

    public async Task<Order?> GetOrderWithItemsAsync(int orderId)
    {
        var sql = @"SELECT o.id, o.userid, o.total, o.createdat, o.updatedat,
                           oi.id, oi.orderid, oi.productid, oi.quantity, oi.unitprice, oi.createdat, oi.updatedat,
                           p.id, p.name, p.sku, p.price, p.stock, p.category, p.createdat, p.updatedat
                    FROM orders o
                    INNER JOIN orderitems oi ON o.id = oi.orderid
                    INNER JOIN products p ON oi.productid = p.id
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
        var sql = "SELECT * FROM orders WHERE userid=@UserId";
        return await _connection.QueryAsync<Order>(sql, new { UserId = userId }, _transaction);
    }
}