using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PlattformChallenge.Core.Interfaces
{
    interface IRepository<TEntity,TPrimaryKey> where TEntity:class
    {

        Task<List<TEntity>> GetAllListAsync();

        Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity,bool>> predicate);

        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity,bool>> predicate);

        Task<TEntity> InsertAsync(TEntity entity);

        Task<TEntity> UpdateAsync(TEntity entity);

        Task DeleteAsync(TEntity entity);

        Task DeleteAsync(Expression<Func<TEntity,bool>> predicate);
    }
}
