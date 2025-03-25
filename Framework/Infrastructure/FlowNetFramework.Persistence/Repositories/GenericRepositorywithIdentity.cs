using FlowNetFramework.Application.Abstractions.Repositories;
using FlowNetFramework.Commons.Helpers;
using FlowNetFramework.Commons.Models.Responses;
using FlowNetFramework.Persistence.Data.Audits;
using FlowNetFramework.Persistence.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace FlowNetFramework.Persistence.Repositories
{
    public class GenericRepositorywithIdentity<T, TContext, TUser, TRole, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> : IGenericRepository<T>
        where TUser : IdentityUser, new()
        where TRole : IdentityRole<string>, new()
        where TUserClaim : IdentityUserClaim<string>, new()
        where TUserRole : IdentityUserRole<string>, new()
        where TUserLogin : IdentityUserLogin<string>, new()
        where TRoleClaim : IdentityRoleClaim<string>, new()
        where TUserToken : IdentityUserToken<string>, new()
        where T : BaseEntity
        where TContext : BaseDbContextwithIdentity<TUser, TRole, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
    {
        private readonly TContext _dbContext;

        private DbSet<T> _dbset;

        public GenericRepositorywithIdentity(TContext dbContext)
        {
            _dbContext = dbContext;
            _dbset = _dbContext.Set<T>();
        }

        #region Read
        public async Task<IQueryable<T>?> Get(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return null;

            IQueryable<T?> query = _dbset.AsNoTracking().Where(x => x.IsActive).OrderByDescending(x => x.Id);

            return query;
        }

        public async Task<IQueryable<T>?> Get(CancellationToken cancellationToken, params Expression<Func<T, object>>[] includes)
        {
            if (cancellationToken.IsCancellationRequested) return null;

            IQueryable<T?> query = _dbset.AsNoTracking().Where(x => x.IsActive).OrderByDescending(x => x.Id);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query;
        }

        public async Task<T?> GetByGuidIdAsync(CancellationToken cancellationToken, Guid guid)
        {
            if (cancellationToken.IsCancellationRequested) return null;

            return await _dbset.AsNoTracking().Where(x => x.IsActive).OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.Guid == guid);
        }

        public async Task<T?> GetByGuidIdAsync(CancellationToken cancellationToken, Guid guid, params Expression<Func<T, object>>[] includes)
        {
            if (cancellationToken.IsCancellationRequested) return null;

            IQueryable<T?> query = _dbset.AsNoTracking().Where(x => x.IsActive).OrderByDescending(x => x.Id);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(x => x.Guid == guid);
        }

        public async Task<T?> GetSingleAsync(CancellationToken cancellationToken, Expression<Func<T, bool>> filter)
        {
            if (cancellationToken.IsCancellationRequested) return null;

            return await _dbset.FirstOrDefaultAsync(filter);
        }

        public async Task<IQueryable<T>?> GetWithFilter(CancellationToken cancellationToken, Expression<Func<T, bool>> filter)
        {
            if (cancellationToken.IsCancellationRequested) return null;

            return _dbset.AsNoTracking().Where(x => x.IsActive).OrderByDescending(x => x.Id).Where(filter);
        }

        public async Task<IQueryable<T>?> GetwithFilterInclude(CancellationToken cancellationToken, Expression<Func<T, bool>> filter, List<Func<IQueryable<T>, IQueryable<T>>> includeFuncs = null)
        {
            if (cancellationToken.IsCancellationRequested) return null;

            IQueryable<T?> query = _dbset.AsNoTracking().Where(x => x.IsActive).OrderByDescending(x => x.Id);

            if (includeFuncs != null)
                foreach (var includeFunc in includeFuncs)
                {
                    if (includeFunc != null)
                    {
                        query = includeFunc(query);
                    }
                }

            return query.Where(filter);
        }

        public async Task<PagedResponse<List<T>>> GetwithPaginationAsync(CancellationToken cancellationToken, int? pageNumber = null, int? pageSize = null)
        {
            IQueryable<T> query = _dbset.AsNoTracking().Where(x => x.IsActive).OrderByDescending(x => x.Id);

            int totalRecords = await query.CountAsync();

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                query = query.CustomPagination(pageNumber, pageSize);
            }

            List<T> result = await query.ToListAsync();

            return new PagedResponse<List<T>>(result, pageNumber ?? 1, pageSize ?? 10, totalRecords)
            {
                PageNumber = pageNumber ?? 1,
                PageSize = pageSize ?? 10,
                TotalRecords = totalRecords,
                Data = result
            };
        }

        public async Task<PagedResponse<List<T>>> GetAllwithFilterAndPaginationAsync(CancellationToken cancellationToken, Expression<Func<T, bool>> filter = null, List<Func<IQueryable<T>, IQueryable<T>>> includeFuncs = null, int? pageNumber = null, int? pageSize = null)
        {
            if (filter == null)
                filter = x => true;

            IQueryable<T> query = _dbset.AsNoTracking().Where(x => x.IsActive).OrderByDescending(x => x.Id).Where(filter);

            int totalRecords = await query.CountAsync();

            if (includeFuncs != null)
                foreach (var includeFunc in includeFuncs)
                {
                    if (includeFunc != null)
                    {
                        query = includeFunc(query);
                    }
                }

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                query = query.CustomPagination(pageNumber, pageSize);
            }

            List<T> result = await query.ToListAsync();

            return new PagedResponse<List<T>>(result, pageNumber ?? 1, pageSize ?? 10, totalRecords)
            {
                PageNumber = pageNumber ?? 1,
                PageSize = pageSize ?? 10,
                TotalRecords = totalRecords,
                Data = result
            };
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

        public bool SoftDelete(CancellationToken cancellationToken, T entity)
        {
            entity.IsActive = false;

            return Update(cancellationToken, entity);
        }
        #endregion
    }
}
