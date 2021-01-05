using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlattformChallenge.Core.Interfaces;
using PlattformChallenge.Models;

namespace PlattformChallenge.Infrastructure
{
    public class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext _dbcontext;

        public virtual DbSet<TEntity> Table => _dbcontext.Set<TEntity>();

        public RepositoryBase(AppDbContext dbContext)
        {
            this._dbcontext = dbContext;
        }

        public IQueryable<TEntity> GetAll()
        {
            return Table.AsQueryable();
        }

        public List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate)
        {
            return GetAll().Where(predicate).ToList();
        }

        public async Task<List<TEntity>> GetAllListAsync()
        {
            return await Table.AsQueryable().ToListAsync();
        }

        public async Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Table.AsQueryable().Where(predicate).ToListAsync();
        }



        public  TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            var entity =  Table.AsQueryable().FirstOrDefault(predicate);

            return entity;

        }
        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var entity = await Table.AsQueryable().FirstOrDefaultAsync(predicate);

            return entity;
            
        }

        public async Task InsertAsync(TEntity toInsert)
        {
            var entity = await Table.AddAsync(toInsert);

            await _dbcontext.SaveChangesAsync();

         }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            AttachIfNot(entity);
            _dbcontext.Entry(entity).State = EntityState.Modified;
            await _dbcontext.SaveChangesAsync();
            return entity;
        }

        public async Task<TEntity> DeleteAsync(TEntity entity)
        {
            AttachIfNot(entity);
            Table.Remove(entity);
            await _dbcontext.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            foreach (var entity in Table.AsQueryable().Where(predicate).ToList())
            {
                await DeleteAsync(entity);
            }
        }

        public IQueryable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = GetAll().Where(predicate);
            return includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

        public Task<List<TEntity>> FindByAndToListAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
        {
            return FindBy(predicate, includes).ToListAsync();
        }

        public Task<TEntity>  IncludeAndFindOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = GetAll();
            var results =includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
            return results.FirstOrDefaultAsync(predicate);
        }

        protected virtual void AttachIfNot(TEntity entity)
        {
            var entry = _dbcontext.ChangeTracker.Entries()
            .FirstOrDefault(ent => ent.Entity == entity);
            if (entry != null)
            {
                return;
            }
            Table.Attach(entity);
        }

        public bool Exists(Expression<Func<TEntity, bool>> predicate)
        {
            return GetAll().Any(predicate);
        }

        public async Task<PaginatedList<TEntity>> FindByAndCreatePaginateAsync(int pageIndex, int pageSize, Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = FindBy(predicate, includes);
            var source = query.AsNoTracking();
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<TEntity>(items, count, pageIndex, pageSize);
        }
    }
}  
