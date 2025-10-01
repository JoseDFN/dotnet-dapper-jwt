using Application.Interfaces;
using Dapper;
using Domain.Entities;
using System.Data;

namespace Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(IDbConnection connection, IDbTransaction transaction)
        : base(connection, transaction) { }

    public override async Task<int> AddAsync(User user)
    {
        var sql = @"INSERT INTO users (username, passwordhash, roleid, createdat, updatedat)
                    VALUES (@Username, @PasswordHash, @RoleId, @CreatedAt, @UpdatedAt)
                    RETURNING id;";
        var id = await _connection.ExecuteScalarAsync<int>(sql, user, _transaction);
        user.Id = id;
        return id;
    }

    public override async Task UpdateAsync(User user)
    {
        var sql = @"UPDATE users
                    SET username=@Username, passwordhash=@PasswordHash, roleid=@RoleId, updatedat=@UpdatedAt
                    WHERE id=@Id";
        await _connection.ExecuteAsync(sql, user, _transaction);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var sql = "SELECT * FROM users WHERE username=@Username";
        return await _connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username }, _transaction);
    }

    public async Task<User?> GetUserWithRoleAsync(int id)
    {
        var sql = @"SELECT u.id, u.username, u.passwordhash, u.roleid, u.createdat, u.updatedat,
                           r.id, r.name, r.createdat, r.updatedat
                    FROM users u
                    INNER JOIN roles r ON u.roleid = r.id
                    WHERE u.id=@Id";

        User? user = null;

        await _connection.QueryAsync<User, Role, User>(
            sql,
            (u, r) =>
            {
                if (user == null)
                {
                    user = u;
                    user.Role = r;
                }
                return user;
            },
            new { Id = id },
            _transaction,
            splitOn: "id"
        );

        return user;
    }
}