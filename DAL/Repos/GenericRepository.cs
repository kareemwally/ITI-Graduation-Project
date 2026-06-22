using System.Linq.Expressions;
using DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repos
{
    /// <summary>EF Core implementation of <see cref="IGenericRepository{T}"/>.</summary>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly FayedDbContext Context;
        protected readonly DbSet<T> Set;

        public GenericRepository(FayedDbContext context)
        {
            Context = context;
            Set = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id) => await Set.FindAsync(id);

        public virtual async Task<IReadOnlyList<T>> GetAllAsync() =>
            await Set.AsNoTracking().ToListAsync();

        public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
            await Set.AsNoTracking().Where(predicate).ToListAsync();

        public virtual Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
            Set.FirstOrDefaultAsync(predicate);

        public virtual Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
            Set.AnyAsync(predicate);

        public virtual Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) =>
            predicate is null ? Set.CountAsync() : Set.CountAsync(predicate);

        public virtual async Task AddAsync(T entity) => await Set.AddAsync(entity);

        public virtual async Task AddRangeAsync(IEnumerable<T> entities) => await Set.AddRangeAsync(entities);

        public virtual void Update(T entity) => Set.Update(entity);

        public virtual void Remove(T entity) => Set.Remove(entity);

        public virtual void RemoveRange(IEnumerable<T> entities) => Set.RemoveRange(entities);

        public IQueryable<T> Query() => Set.AsQueryable();
    }
}
