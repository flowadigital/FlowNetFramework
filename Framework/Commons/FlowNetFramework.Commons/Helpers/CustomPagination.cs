using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FlowNetFramework.Commons.Helpers;

public static class PaginationQuery
{
    public static IQueryable<T> CustomQuery<T>(this IQueryable<T> query, Expression<Func<T, bool>> filter = null) where T : class
    {
        if (filter != null)
        {
            query = query.Where(filter);
        }

        return query;
    }

    public static IQueryable<T> CustomPagination<T>(this IQueryable<T> query, int? page = 0, int? pageSize = null)
    {
        if (page != null)
        {
            query = query.Skip(((int)page - 1) * (int)pageSize);
        }

        if (pageSize != null)
        {
            query = query.Take((int)pageSize);
        }
        return query;
    }

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string orderBy, bool isDescending)
    {
        if (!string.IsNullOrEmpty(orderBy))
        {
            // Sıralama yönünü belirleme
            if (isDescending)
            {
                query = query.OrderByDescending(e => EF.Property<object>(e, orderBy));
            }
            else
            {
                query = query.OrderBy(e => EF.Property<object>(e, orderBy));
            }
        }

        return query;
    }
}
