using PlattformChallenge.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PlattformChallenge.Core.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetAll();

        List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate);

        Task<List<TEntity>> GetAllListAsync();

        Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate);

        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        Task InsertAsync(TEntity entity);

        Task<TEntity> UpdateAsync(TEntity entity);

        Task<TEntity> DeleteAsync(TEntity entity);

        Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);
        void AsNoTracking();

    }
}
