using Application.Interfaces;
using Infrastructure.Repositories;
using System.Data;

namespace Infrastructure;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction _transaction;

    private IProductRepository? _products;
    private IOrderRepository? _orders;
    private IOrderItemRepository? _orderItems;
    private IUserRepository? _users;
    private IRoleRepository? _roles;

    public UnitOfWork(IDbConnection connection)
    {
        _connection = connection;
        _connection.Open();
        _transaction = _connection.BeginTransaction();
    }

    public IProductRepository Products => _products ??= new ProductRepository(_connection, _transaction);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_connection, _transaction);
    public IOrderItemRepository OrderItems => _orderItems ??= new OrderItemRepository(_connection, _transaction);
    public IUserRepository Users => _users ??= new UserRepository(_connection, _transaction);
    public IRoleRepository Roles => _roles ??= new RoleRepository(_connection, _transaction);

    public Task<int> SaveAsync()
    {
        try
        {
            _transaction.Commit();
            return Task.FromResult(1);
        }
        catch
        {
            _transaction.Rollback();
            throw;
        }
        finally
        {
            
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _connection?.Dispose();
    }
}