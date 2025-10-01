using Application.Interfaces;
using Dapper;
using Domain.Entities;
using System.Data;

namespace Infrastructure.Repositories;

public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
{
    public OrderItemRepository(IDbConnection connection, IDbTransaction transaction)
        : base(connection, transaction) { }

    public override async Task<int> AddAsync(OrderItem item)
    {
        var sql = @"INSERT INTO orderitems (orderid, productid, quantity, unitprice, createdat, updatedat)
                    VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @CreatedAt, @UpdatedAt)
                    RETURNING id;";
        var id = await _connection.ExecuteScalarAsync<int>(sql, item, _transaction);
        item.Id = id;
        return id;
    }

    public override async Task UpdateAsync(OrderItem item)
    {
        var sql = @"UPDATE orderitems 
                    SET quantity=@Quantity, unitprice=@UnitPrice, updatedat=@UpdatedAt
                    WHERE id=@Id";
        await _connection.ExecuteAsync(sql, item, _transaction);
    }

    public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId)
    {
        var sql = "SELECT * FROM orderitems WHERE orderid=@OrderId";
        return await _connection.QueryAsync<OrderItem>(sql, new { OrderId = orderId }, _transaction);
    }
}