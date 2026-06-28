using DAL.Data;
using DAL.Repos;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;

namespace DAL.UnitOfWork
{
    /// <summary>EF Core implementation of <see cref="IUnitOfWork"/>.</summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FayedDbContext _context;
        private readonly ConcurrentDictionary<Type, object> _repositories = new();

        public UnitOfWork(FayedDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> Repository<T>() where T : class =>
            (IGenericRepository<T>)_repositories.GetOrAdd(
                typeof(T), _ => new GenericRepository<T>(_context));

        public async Task<IDbContextTransaction> BeginTransactionAsync() =>
    await _context.Database.BeginTransactionAsync();

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

        public ValueTask DisposeAsync() => _context.DisposeAsync();
    }
}
