using Microsoft.EntityFrameworkCore.Storage;

namespace FlowNetFramework.Application.Abstractions.Repositories.Uow;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransaction();
    Task<int> SaveChangesAsync();
    Task Commit();
    void Dispose();
}
