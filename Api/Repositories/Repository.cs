using MinimalApi.Dominio.Entidades;
using MinimalApi.Infraestrutura.Db;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MinimalApi.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbContexto _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger<Repository<T>> _logger;

    public Repository(DbContexto context, ILogger<Repository<T>> logger)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _logger = logger;
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar entidade por ID: {Id}", id);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todas as entidades");
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar entidades com filtro");
            throw;
        }
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar entidade");
            throw;
        }
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbSet.Update(entity);
            return await Task.FromResult(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar entidade");
            throw;
        }
    }

    public virtual async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            _dbSet.Remove(entity);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar entidade com ID: {Id}", id);
            throw;
        }
    }

    public virtual async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken) != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência da entidade com ID: {Id}", id);
            throw;
        }
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao contar entidades");
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar entidades paginadas");
            throw;
        }
    }
}

// Unit of Work Pattern
public interface IUnitOfWork : IDisposable
{
    IRepository<Veiculo> Veiculos { get; }
    IRepository<Administrador> Administradores { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContexto _context;
    private readonly IServiceProvider _serviceProvider;
    private bool _disposed = false;

    public UnitOfWork(DbContexto context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    public IRepository<Veiculo> Veiculos => 
        _serviceProvider.GetRequiredService<IRepository<Veiculo>>();

    public IRepository<Administrador> Administradores => 
        _serviceProvider.GetRequiredService<IRepository<Administrador>>();

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.RollbackTransactionAsync(cancellationToken);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
