using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlattformChallenge.Core.Interfaces;

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
    }
}  
