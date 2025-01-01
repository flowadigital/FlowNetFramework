using System.Linq.Expressions;

namespace FlowNetFramework.Core.Abstractions
{
    public interface IRepository<T>
        where T : class
    {
        #region Yeni

        #region Read
        IQueryable<T> Get(CancellationToken cancellationToken);

        public IQueryable<T> Get(CancellationToken cancellationToken, params Expression<Func<T, object>>[] includes);

        public IQueryable<T> GetWhere(CancellationToken cancellationToken, Expression<Func<T, bool>> filter);

        public IQueryable<T> GetWhere(CancellationToken cancellationToken, Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includes);

        public Task<T> GetSingleAsync(CancellationToken cancellationToken, Expression<Func<T, bool>> filter);

        public Task<T?> GetByGuidIdAsync(CancellationToken cancellationToken, Guid id);

        public Task<T?> GetByGuidIdAsync(CancellationToken cancellationToken, Guid id, params Expression<Func<T, object>>[] includes); 
        #endregion

        #region Write
        public Task<bool> AddAsync(CancellationToken cancellationToken, T entity);

        public Task<bool> AddRangeAsync(CancellationToken cancellationToken, List<T> entities);

        public bool Update(CancellationToken cancellationToken, T entity);

        public bool Delete(CancellationToken cancellationToken, T entity);

        public Task<bool> DeleteAsync(CancellationToken cancellationToken, Guid id);

        public bool DeleteRange(CancellationToken cancellationToken, List<T> entities);

        #endregion

        #endregion

        #region Eski
        //Task<PagedResponse<List<T>>> GetAllAsync(int? pageNumber = null, int? pageSize = null);
        //Task<List<T>> GetAllwithNopaginationAsync();
        //Task<PagedResponse<List<T>>> GetAllwithFilterAndPaginationAsync(Expression<Func<T, bool>> filter = null, List<Func<IQueryable<T>, IQueryable<T>>> includeFuncs = null, int? pageNumber = null, int? pageSize = null);
        //Task<List<T>> GetAllwithFilterAsync(Expression<Func<T, bool>> filter = null, List<Func<IQueryable<T>, IQueryable<T>>> includeFuncs = null);
        //Task<PagedResponse<List<T>>> GetListByIdAsync(long id, int? pageNumber = null, int? pageSize = null);
        //Task<PagedResponse<List<T>>> GetListByGuidAsync(Guid guid, int? pageNumber = null, int? pageSize = null);
        //IQueryable<T> GetAllQuery();
        //Task<T> GetByGuidAsync(Guid guid);
        //Task<T> GetByGuidAsyncInclude(Guid guid, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);
        //Task<T?> GetByIdAsync(long id);
        //Task AddAsync(T entity);
        //Task AddRangeAsync(IEnumerable<T> entity);
        //void SoftDelete(T entity);
        //void Delete(T entity);
        //void DeleteRange(List<T> entities);
        //void Update(T entity);
        //void UpdateRange(List<T> entity);
        //Task<bool> AnyAsync(Expression<Func<T, bool>> expression);
        //Task<bool> Contains(T entity); 
        #endregion
    }
}
