using System.Linq.Expressions;

namespace DAL.Repos
{
    /// <summary>
    /// Generic, entity-agnostic data access contract. Depending on this abstraction
    /// (not on <c>FayedDbContext</c>) keeps the business layer decoupled from EF Core (DIP).
    /// </summary>
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);

        /// <summary>Escape hatch for composing complex, entity-specific queries (paging, includes, projections).</summary>
        IQueryable<T> Query();
    }
}
