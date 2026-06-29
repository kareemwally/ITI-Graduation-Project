using DAL.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.UnitOfWork
{
    /// <summary>
    /// Coordinates work across repositories within a single transaction/DbContext instance
    /// and commits it atomically. Repositories are resolved lazily and cached per instance.
    /// </summary>
    public interface IUnitOfWork : IAsyncDisposable
    {
        IGenericRepository<T> Repository<T>() where T : class;
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> SaveChangesAsync();
    }
}
