using FlowNetFramework.Application.Abstractions.Repositories;
using FlowNetFramework.Persistence.Data.Audits;
using FlowNetFramework.Persistence.Data.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace FlowNetFramework.Persistence.Repositories
{
    public class GenericRepository<T, TContext> : IRepository<T>
        where T : BaseEntity
        where TContext : BaseDbContext
    {
        private readonly TContext _dbContext;

        private DbSet<T> _dbset;

        public GenericRepository(TContext dbContext)
        {
            _dbContext = dbContext;
            _dbset = _dbContext.Set<T>();
        }

        #region Read
        public IQueryable<T> Get(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return null;

            IQueryable<T?> query = _dbset;

            return query;
        }

        public IQueryable<T> Get(CancellationToken cancellationToken, params Expression<Func<T, object>>[] includes)
        {
            if (cancellationToken.IsCancellationRequested) return null;

            IQueryable<T?> query = _dbset;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query;
        }

        public async Task<T?> GetByGuidIdAsync(CancellationToken cancellationToken, Guid guid)
        {
            if (cancellationToken.IsCancellationRequested) return null;

            return await _dbset.FirstOrDefaultAsync(x => x.Guid == guid);
        }

        public async Task<T?> GetByGuidIdAsync(CancellationToken cancellationToken, Guid guid, params Expression<Func<T, object>>[] includes)
        {
            if (cancellationToken.IsCancellationRequested) return null;

            IQueryable<T?> query = _dbset;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(x => x.Guid == guid);
        }

        public async Task<T> GetSingleAsync(CancellationToken cancellationToken, Expression<Func<T, bool>> filter)
        {
            if (cancellationToken.IsCancellationRequested) return null;

            return await _dbset.FirstOrDefaultAsync(filter);
        }

        public IQueryable<T> GetWithFilter(CancellationToken cancellationToken, Expression<Func<T, bool>> filter)
        {
            if (cancellationToken.IsCancellationRequested) return null;

            return _dbset.Where(filter);
        }

        public IQueryable<T> GetwithFilterInclude(CancellationToken cancellationToken, Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includes)
        {
            if (cancellationToken.IsCancellationRequested) return null;

            IQueryable<T?> query = _dbset;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query.Where(filter);
        }

        //public async Task<PagedResponse<List<T>>> GetwithPagenationAsync(int? pageNumber = null, int? pageSize = null)
        //{
        //    IQueryable<T> query = _dbset.AsNoTracking().Where(x => x.IsActive).OrderByDescending(x => x.Id);

        //    int totalRecords = await query.CountAsync();

        //    if (pageNumber.HasValue && pageSize.HasValue)
        //    {
        //        query = query.CustomPagination(pageNumber, pageSize);
        //    }

        //    List<T> result = await query.ToListAsync();

        //    return new PagedResponse<List<T>>(result, pageNumber ?? 1, pageSize ?? 10, totalRecords)
        //    {
        //        PageNumber = pageNumber ?? 1,
        //        PageSize = pageSize ?? 10,
        //        TotalRecords = totalRecords,
        //        Data = result
        //    };
        //}
        #endregion

        #region Write
        public async Task<bool> AddAsync(CancellationToken cancellationToken, T entity)
        {
            if (cancellationToken.IsCancellationRequested) return false;

            EntityEntry<T> entityEntry = await _dbset.AddAsync(entity);

            return entityEntry.State == EntityState.Added;
        }

        public async Task<bool> AddRangeAsync(CancellationToken cancellationToken, List<T> entities)
        {
            if (cancellationToken.IsCancellationRequested) return false;

            await _dbset.AddRangeAsync(entities);

            return true;
        }

        public bool Delete(CancellationToken cancellationToken, T entity)
        {
            if (cancellationToken.IsCancellationRequested) return false;

            EntityEntry<T> entityEntry = _dbset.Remove(entity);

            return entityEntry.State == EntityState.Deleted;
        }

        public async Task<bool> DeleteAsync(CancellationToken cancellationToken, Guid guid)
        {
            if (cancellationToken.IsCancellationRequested) return false;

            T entity = await _dbset.FirstOrDefaultAsync(x => x.Guid == guid);

            return Delete(cancellationToken, entity);
        }

        public bool DeleteRange(CancellationToken cancellationToken, List<T> entities)
        {
            if (cancellationToken.IsCancellationRequested) return false;

            _dbset.RemoveRange(entities);

            return true;
        }

        public bool Update(CancellationToken cancellationToken, T entity)
        {
            if (cancellationToken.IsCancellationRequested) return false;

            EntityEntry<T> entityEntry = _dbset.Update(entity);

            return entityEntry.State == EntityState.Modified;
        }      
        #endregion
    }
}
