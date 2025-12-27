using FlowNetFramework.Commons.Models.Responses;
using System.Linq.Expressions;

namespace FlowNetFramework.Application.Abstractions.Repositories;
public interface IGenericRepository<T>
    where T : class
{
    #region Read
    Task<IQueryable<T>?> Get(
        CancellationToken cancellationToken
    );

    Task<IQueryable<T>?> Get(
        CancellationToken cancellationToken,
        params Expression<Func<T, object>>[] includes
    );

    Task<T?> GetByGuidIdAsync(
        CancellationToken cancellationToken,
        Guid guid
    );

    Task<T?> GetByGuidIdAsync(
        CancellationToken cancellationToken,
        Guid guid,
        params Expression<Func<T, object>>[] includes
    );

    Task<T?> GetSingleAsync(
        CancellationToken cancellationToken,
        Expression<Func<T, bool>> filter,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null
    );

    Task<IQueryable<T>?> GetWithFilter(
        CancellationToken cancellationToken,
        Expression<Func<T, bool>> filter,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null
    );

    Task<IQueryable<T>?> GetwithFilterInclude(
        CancellationToken cancellationToken,
        Expression<Func<T, bool>> filter,
        List<Func<IQueryable<T>, IQueryable<T>>>? includeFuncs = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null
    );

    Task<PagedResponse<List<T>>> GetwithPaginationAsync(
        CancellationToken cancellationToken,
        int? pageNumber = null,
        int? pageSize = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null
    );

    Task<PagedResponse<List<T>>> GetAllwithFilterAndPaginationAsync(
        CancellationToken cancellationToken,
        Expression<Func<T, bool>>? filter = null,
        List<Func<IQueryable<T>, IQueryable<T>>>? includeFuncs = null,
        int? pageNumber = null,
        int? pageSize = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null
    );

    #endregion

    #region Write
    public Task<bool> AddAsync(CancellationToken cancellationToken, T entity);

    public Task<bool> AddRangeAsync(CancellationToken cancellationToken, List<T> entities);

    public bool Update(CancellationToken cancellationToken, T entity);

    public bool Delete(CancellationToken cancellationToken, T entity);

    public bool SoftDelete(CancellationToken cancellationToken, T entity);

    public bool SoftDeleteRange(CancellationToken cancellationToken, List<T> entities);

    public Task<bool> DeleteAsync(CancellationToken cancellationToken, Guid id);

    public bool DeleteRange(CancellationToken cancellationToken, List<T> entities);

    #endregion
}
