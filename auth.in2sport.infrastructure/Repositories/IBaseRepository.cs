using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace auth.in2sport.infrastructure.Repositories
{
    public interface IBaseRepository<TEntity>
    {
        Task<bool> CreateAsync(TEntity entity);
        Task<List<TEntity>> GetAsync();
        Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter);
        Task<bool> UpdateAsync(TEntity entity);
        Task<TEntity> GetByIdAsync(Guid id);
        Task<TEntity> GetByEmailAsync(string email);
        Task<bool> DeleteAsync(object id);
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
