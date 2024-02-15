using auth.in2sport.infrastructure.Repositories.Postgres.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace auth.in2sport.infrastructure.Repositories.Postgres
{
    public class PostgresRepository<TEntity> : IBaseRepository<TEntity> where TEntity : PostgresEntity
    {
        #region Private Properties

        /// <summary>
        /// Instance of the Postgres Repository
        /// </summary>
        private readonly PostgresDbContext _dbContext;

        #endregion

        #region Constructor

        /// <summary>
        /// Defines constructor
        /// </summary>
        /// <param name="dbContext"></param>
        public PostgresRepository(PostgresDbContext dbContext)
        {

            this._dbContext = dbContext;
        }
        #endregion

        public async Task<bool> CreateAsync(TEntity entity)
        {
            await _dbContext.AddAsync(entity);
            var result = await _dbContext.SaveChangesAsync();

            return result > 0;
        }       

        public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter)
        {
            var results = await _dbContext.Set<TEntity>().Where(filter).ToListAsync();
            return results;
        }

        public async Task<List<TEntity>> GetAsync()
        {
            var results = await _dbContext.Set<TEntity>().ToListAsync<TEntity>();
            return results;
        }
        public async Task<TEntity> GetByIdAsync(Guid id)
        {
            var result = await _dbContext.Set<TEntity>().FindAsync(id);
            return result;
        }

        public async Task<TEntity> GetByEmailAsync(string email)
        {
            var entityType = typeof(TEntity);
            var emailProperty = entityType.GetProperty("Email");

            if (emailProperty == null)
            {
                throw new ArgumentException("The entity does not contain an 'Email' property.");
            }

            var entities = await _dbContext.Set<TEntity>().ToListAsync();

            var result = entities.FirstOrDefault(u => emailProperty.GetValue(u) != null && emailProperty.GetValue(u).ToString() == email);

            return result;
        }

        public async Task<bool> UpdateAsync(TEntity entity)
        {
            _dbContext.Set<TEntity>().Update(entity);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAsync(object id)
        {
            var entityToDelete = await _dbContext.Set<TEntity>().FindAsync(id);
            if (entityToDelete == null)
                return false;

            _dbContext.Set<TEntity>().Remove(entityToDelete);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }
    }
}
