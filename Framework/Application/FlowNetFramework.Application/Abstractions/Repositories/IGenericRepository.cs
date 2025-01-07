using FlowNetFramework.Commons.Models.Responses;
using System.Linq.Expressions;

namespace FlowNetFramework.Application.Abstractions.Repositories;
public interface IGenericRepository<T>
    where T : class
{
    #region Yeni

    #region Read
    Task<IQueryable<T>?> Get(CancellationToken cancellationToken);

    public Task<IQueryable<T>?> Get(CancellationToken cancellationToken, params Expression<Func<T, object>>[] includes);

    public Task<IQueryable<T>?> GetWithFilter(CancellationToken cancellationToken, Expression<Func<T, bool>> filter);

    public Task<IQueryable<T>?> GetwithFilterInclude(CancellationToken cancellationToken, Expression<Func<T, bool>> filter, List<Func<IQueryable<T>, IQueryable<T>>> includeFuncs = null);

    public Task<T> GetSingleAsync(CancellationToken cancellationToken, Expression<Func<T, bool>> filter);

    public Task<T?> GetByGuidIdAsync(CancellationToken cancellationToken, Guid id);

    public Task<T?> GetByGuidIdAsync(CancellationToken cancellationToken, Guid id, params Expression<Func<T, object>>[] includes);

    public Task<PagedResponse<List<T>>> GetwithPaginationAsync(CancellationToken cancellationToken, int? pageNumber = null, int? pageSize = null);

    public Task<PagedResponse<List<T>>> GetAllwithFilterAndPaginationAsync(CancellationToken cancellationToken, Expression<Func<T, bool>> filter = null, List<Func<IQueryable<T>, IQueryable<T>>> includeFuncs = null, int? pageNumber = null, int? pageSize = null);

    #endregion

    #region Write
    public Task<bool> AddAsync(CancellationToken cancellationToken, T entity);

    public Task<bool> AddRangeAsync(CancellationToken cancellationToken, List<T> entities);

    public bool Update(CancellationToken cancellationToken, T entity);

    public bool Delete(CancellationToken cancellationToken, T entity);

    public bool SoftDelete(CancellationToken cancellationToken, T entity);

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
