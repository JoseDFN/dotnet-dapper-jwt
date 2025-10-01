using Application.Interfaces;
using Dapper;
using Domain.Entities;
using System.Data;

namespace Infrastructure.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(IDbConnection connection, IDbTransaction transaction)
        : base(connection, transaction) { }

    public override async Task<int> AddAsync(Product product)
    {
        var sql = @"INSERT INTO products (name, sku, price, stock, category, createdat, updatedat)
                    VALUES (@Name, @Sku, @Price, @Stock, @Category, @CreatedAt, @UpdatedAt)
                    RETURNING id;";
        var id = await _connection.ExecuteScalarAsync<int>(sql, product, _transaction);
        product.Id = id;
        return id;
    }

    public override async Task UpdateAsync(Product product)
    {
        var sql = @"UPDATE products 
                    SET name=@Name, sku=@Sku, price=@Price, stock=@Stock, category=@Category, updatedat=@UpdatedAt
                    WHERE id=@Id";
        await _connection.ExecuteAsync(sql, product, _transaction);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(string? category = null, string? name = null)
    {
        var sql = "SELECT * FROM products WHERE 1=1";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(category))
        {
            sql += " AND category=@Category";
            parameters.Add("Category", category);
        }

        if (!string.IsNullOrEmpty(name))
        {
            sql += " AND name ILIKE @Name"; // ILIKE para case-insensitive en Postgres
            parameters.Add("Name", $"%{name}%");
        }

        return await _connection.QueryAsync<Product>(sql, parameters, _transaction);
    }
}