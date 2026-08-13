using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Common;

namespace NurseryManagementSystem.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext Context;
        protected readonly DbSet<T> DbSet;

        public GenericRepository(AppDbContext context)
        {
            Context = context;
            DbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await DbSet.FindAsync(new object?[] { id }, cancellationToken);

        public async Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default)
            => await DbSet.AsNoTracking().ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<T>> ListAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
            => await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

        public async Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
            => await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);

        public async Task<bool> AnyAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
            => await DbSet.AnyAsync(predicate, cancellationToken);

        public async Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
            => predicate is null
                ? await DbSet.CountAsync(cancellationToken)
                : await DbSet.CountAsync(predicate, cancellationToken);

        public IQueryable<T> Query() => DbSet.AsQueryable();

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
            => await DbSet.AddAsync(entity, cancellationToken);

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
            => await DbSet.AddRangeAsync(entities, cancellationToken);

        public void Update(T entity) => DbSet.Update(entity);

        public void Remove(T entity) => DbSet.Remove(entity);

        public void RemoveRange(IEnumerable<T> entities) => DbSet.RemoveRange(entities);
    }
}
