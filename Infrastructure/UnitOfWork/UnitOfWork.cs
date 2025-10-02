using Application.Interfaces;
using Infrastructure.Repositories;
using System.Data;
using Microsoft.Extensions.Logging;
using Domain.Exceptions;

namespace Infrastructure;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction _transaction;
    private readonly ILogger<UnitOfWork> _logger;
    private bool _disposed = false;
    private bool _transactionCommitted = false;

    private IProductRepository? _products;
    private IOrderRepository? _orders;
    private IOrderItemRepository? _orderItems;
    private IUserRepository? _users;
    private IRoleRepository? _roles;

    public UnitOfWork(IDbConnection connection, ILogger<UnitOfWork> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        try
        {
            _connection.Open();
            _transaction = _connection.BeginTransaction();
            _logger.LogDebug("Database transaction started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start database transaction");
            throw new BusinessException("DATABASE_CONNECTION_ERROR", "Failed to establish database connection", ex);
        }
    }

    public IProductRepository Products => _products ??= new ProductRepository(_connection, _transaction);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_connection, _transaction);
    public IOrderItemRepository OrderItems => _orderItems ??= new OrderItemRepository(_connection, _transaction);
    public IUserRepository Users => _users ??= new UserRepository(_connection, _transaction);
    public IRoleRepository Roles => _roles ??= new RoleRepository(_connection, _transaction);

    public async Task<int> SaveAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(UnitOfWork));

        if (_transactionCommitted)
        {
            _logger.LogWarning("Attempted to save after transaction was already committed");
            return 1;
        }

        try
        {
            _transaction.Commit();
            _transactionCommitted = true;
            _logger.LogDebug("Database transaction committed successfully");
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to commit database transaction, rolling back");
            try
            {
                _transaction.Rollback();
                _logger.LogDebug("Database transaction rolled back successfully");
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Failed to rollback database transaction");
            }
            
            throw new BusinessException("TRANSACTION_COMMIT_ERROR", "Failed to save changes to database", ex);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                if (!_transactionCommitted && _transaction != null)
                {
                    _logger.LogWarning("UnitOfWork disposed without committing transaction, rolling back");
                    _transaction.Rollback();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during transaction rollback in dispose");
            }
            finally
            {
                _transaction?.Dispose();
                _connection?.Dispose();
                _disposed = true;
                _logger.LogDebug("UnitOfWork disposed successfully");
            }
        }
    }
}