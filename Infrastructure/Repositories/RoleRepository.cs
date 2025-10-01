using Application.Interfaces;
using Dapper;
using Domain.Entities;
using System.Data;

namespace Infrastructure.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(IDbConnection connection, IDbTransaction transaction)
        : base(connection, transaction) { }

    public override async Task<int> AddAsync(Role role)
    {
        var sql = @"INSERT INTO roles (name, createdat, updatedat)
                    VALUES (@Name, @CreatedAt, @UpdatedAt)
                    RETURNING id;";
        var id = await _connection.ExecuteScalarAsync<int>(sql, role, _transaction);
        role.Id = id;
        return id;
    }

    public override async Task UpdateAsync(Role role)
    {
        var sql = @"UPDATE roles
                    SET name=@Name, updatedat=@UpdatedAt
                    WHERE id=@Id";
        await _connection.ExecuteAsync(sql, role, _transaction);
    }

    public async Task<Role?> GetByNameAsync(string roleName)
    {
        var sql = "SELECT * FROM roles WHERE name=@RoleName";
        return await _connection.QueryFirstOrDefaultAsync<Role>(sql, new { RoleName = roleName }, _transaction);
    }
}