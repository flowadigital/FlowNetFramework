using FlowNetFramework.Application.Abstractions.Repositories;
using FlowNetFramework.Commons.Models.Responses;
using FlowNetFramework.Persistence.Data.Audits;
using FlowNetFramework.Persistence.Data.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using FlowNetFramework.Commons.Helpers;

namespace FlowNetFramework.Persistence.Repositories
{
    public class GenericRepository<T, TContext> : IGenericRepository<T>
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
        public async Task<IQueryable<T>?> Get(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            // 🔹 FILTER
            return _dbset
                .AsNoTracking()
                .Where(x => x.IsActive);
        }

        public async Task<IQueryable<T>?> Get(
             CancellationToken cancellationToken,
             params Expression<Func<T, object>>[] includes
         )
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            // 🔹 FILTER
            IQueryable<T> query = _dbset
                .AsNoTracking()
                .Where(x => x.IsActive);

            // 🔹 INCLUDE
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return query;
        }

        public async Task<T?> GetByGuidIdAsync(
            CancellationToken cancellationToken,
            Guid guid
        )
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            // 🔹 FILTER
            return await _dbset
                .AsNoTracking()
                .Where(x => x.IsActive)
                .FirstOrDefaultAsync(x => x.Id == guid, cancellationToken);
        }

        public async Task<T?> GetByGuidIdAsync(
            CancellationToken cancellationToken,
            Guid guid,
            params Expression<Func<T, object>>[] includes
        )
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            // 🔹 FILTER
            IQueryable<T> query = _dbset
                .AsNoTracking()
                .Where(x => x.IsActive);

            // 🔹 INCLUDE
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync(x => x.Id == guid, cancellationToken);
        }

        public async Task<T?> GetSingleAsync(
            CancellationToken cancellationToken,
            Expression<Func<T, bool>> filter,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null
        )
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            // 🔹 FILTER
            IQueryable<T> query = _dbset
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Where(filter);

            // 🔹 ORDER BY (opsiyonel)
            query = orderBy != null
                ? orderBy(query)
                : query.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate);

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IQueryable<T>?> GetWithFilter(
            CancellationToken cancellationToken,
            Expression<Func<T, bool>> filter,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null
        )
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            // 🔹 FILTER
            IQueryable<T> query = _dbset
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Where(filter);

            // 🔹 DEFAULT ORDER
            query = orderBy != null
                ? orderBy(query)
                : query.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate);

            return query;
        }

        public async Task<IQueryable<T>?> GetwithFilterInclude(
            CancellationToken cancellationToken,
            Expression<Func<T, bool>> filter,
            List<Func<IQueryable<T>, IQueryable<T>>>? includeFuncs = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null
        )
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            // 🔹 FILTER
            IQueryable<T> query = _dbset
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Where(filter);

            // 🔹 INCLUDE
            if (includeFuncs != null)
            {
                foreach (var includeFunc in includeFuncs)
                {
                    if (includeFunc != null)
                        query = includeFunc(query);
                }
            }

            // 🔹 ORDER BY (opsiyonel)
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return query;
        }

        public async Task<PagedResponse<List<T>>> GetwithPaginationAsync(
            CancellationToken cancellationToken,
            int? pageNumber = null,
            int? pageSize = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            // 🔹 FILTER
            IQueryable<T> query = _dbset
                .AsNoTracking()
                .Where(x => x.IsActive);

            // 🔹 ORDER BY (opsiyonel)
            query = orderBy != null
                ? orderBy(query)
                : query.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate);

            int totalRecords = await query.CountAsync(cancellationToken);

            // 🔹 PAGINATION
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                query = query.CustomPagination(pageNumber, pageSize);
            }

            List<T> result = await query.ToListAsync(cancellationToken);

            return new PagedResponse<List<T>>(
                result,
                pageNumber ?? 1,
                pageSize ?? 10,
                totalRecords
            );
        }

        public async Task<PagedResponse<List<T>>> GetAllwithFilterAndPaginationAsync(
            CancellationToken cancellationToken,
            Expression<Func<T, bool>>? filter = null,
            List<Func<IQueryable<T>, IQueryable<T>>>? includeFuncs = null,
            int? pageNumber = null,
            int? pageSize = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            filter ??= x => true;

            // 🔹 FILTER
            IQueryable<T> query = _dbset
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Where(filter);

            // 🔹 INCLUDE
            if (includeFuncs != null)
            {
                foreach (var includeFunc in includeFuncs)
                {
                    if (includeFunc != null)
                        query = includeFunc(query);
                }
            }

            // 🔹 ORDER
            query = orderBy != null
                ? orderBy(query)
                : query.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate);

            int totalRecords = await query.CountAsync(cancellationToken);

            // 🔹 PAGINATION
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                query = query.CustomPagination(pageNumber, pageSize);
            }

            List<T> result = await query.ToListAsync(cancellationToken);

            return new PagedResponse<List<T>>(
                result,
                pageNumber ?? 1,
                pageSize ?? 10,
                totalRecords
            );
        }
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

            T entity = await _dbset.FirstOrDefaultAsync(x => x.Id == guid);

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

        public bool SoftDelete(CancellationToken cancellationToken, T entity)
        {
            entity.IsActive = false;

            return Update(cancellationToken, entity);
        }

        public bool SoftDeleteRange(CancellationToken cancellationToken, List<T> entities)
        {
            if (cancellationToken.IsCancellationRequested || entities == null || !entities.Any())
                return false;

            foreach (var entity in entities)
            {
                entity.IsActive = false;
            }

            _dbset.UpdateRange(entities);

            return true;
        }
        #endregion
    }
}
