using Application.Interfaces;
using Dapper;
using Domain.Entities;
using System.Data;

namespace Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly IDbConnection _connection;
    protected readonly IDbTransaction _transaction;
    protected readonly string _tableName;

    public GenericRepository(IDbConnection connection, IDbTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
        _tableName = typeof(T).Name.ToLower() + "s"; // convención: User -> users
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        var sql = $"SELECT * FROM {_tableName}";
        return await _connection.QueryAsync<T>(sql, transaction: _transaction);
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        var sql = $"SELECT * FROM {_tableName} WHERE id=@Id";
        return await _connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id }, _transaction);
    }

    public virtual Task<int> AddAsync(T entity)
    {
        throw new NotImplementedException("Implementa AddAsync en repositorios específicos.");
    }

    public virtual Task UpdateAsync(T entity)
    {
        throw new NotImplementedException("Implementa UpdateAsync en repositorios específicos.");
    }

    public virtual async Task DeleteAsync(int id)
    {
        var sql = $"DELETE FROM {_tableName} WHERE id=@Id";
        await _connection.ExecuteAsync(sql, new { Id = id }, _transaction);
    }
}