using Application.Interfaces;
using Dapper;
using Domain.Entities;
using System.Data;

namespace Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(IDbConnection connection, IDbTransaction transaction)
        : base(connection, transaction) { }

    public async Task<(int UserId, string RoleName)?> AuthenticateAsync(string username, string passwordHash)
    {
        var sql = "SELECT * FROM auth_user(@Username, @PasswordHash)";
        var result = await _connection.QueryFirstOrDefaultAsync<(int UserId, string RoleName)>(
            sql,
            new { Username = username, PasswordHash = passwordHash },
            _transaction
        );

        if (result.UserId == 0) return null;
        return result;
    }


    public override async Task<int> AddAsync(User user)
    {
        var sql = @"INSERT INTO users (username, password_hash, role_id, created_at, updated_at)
                VALUES (@Username, @PasswordHash, @RoleId, @Created_at, @Updated_at)
                RETURNING id;";
        var id = await _connection.ExecuteScalarAsync<int>(sql, new
        {
            user.Username,
            user.PasswordHash,
            user.RoleId,
            Created_at = user.created_at,
            Updated_at = user.updated_at
        }, _transaction);
        user.Id = id;
        return id;
    }

    public override async Task UpdateAsync(User user)
    {
        var sql = @"UPDATE users
                SET username=@Username, password_hash=@PasswordHash, role_id=@RoleId, updated_at=@Updated_at
                WHERE id=@Id";
        await _connection.ExecuteAsync(sql, new
        {
            user.Username,
            user.PasswordHash,
            user.RoleId,
            user.Id,
            Updated_at = user.updated_at
        }, _transaction);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var sql = @"SELECT 
                    id, 
                    username, 
                    password_hash AS PasswordHash, 
                    role_id AS RoleId, 
                    created_at AS Created_At, 
                    updated_at AS Updated_At
                FROM users 
                WHERE username = @Username";

        return await _connection.QueryFirstOrDefaultAsync<User>(
            sql,
            new { Username = username },
            _transaction
        );
    }

    public async Task<User?> GetUserWithRoleAsync(int id)
    {
        var sql = @"SELECT u.id, u.username, u.password_hash, u.role_id, u.created_at, u.updated_at,
                           r.id, r.name, r.Created_At, r.Updated_At
                    FROM users u
                    INNER JOIN roles r ON u.role_id = r.id
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