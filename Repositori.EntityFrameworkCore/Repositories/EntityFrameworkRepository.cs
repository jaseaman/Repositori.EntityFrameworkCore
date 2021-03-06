using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositori.Core.Model;
using Repositori.Core.Repositories;

namespace Repositori.EntityFrameworkCore.Repositories
{
    /// <summary>
    /// Implementation of the repository pattern encapsulating entity framework
    /// </summary>
    /// <typeparam name="TEntity">The data object type</typeparam>
    /// <typeparam name="TIdentifier">The data object identifier type</typeparam>
    public class EntityFrameworkRepository<TEntity, TIdentifier> : IRepository<TEntity, TIdentifier>
        where TEntity : class, IIdentifiable<TIdentifier>
    {
        /// <summary>
        /// Database context used for repository actions
        /// </summary>
        protected readonly DbContext Context;

        /// <summary>
        /// Constructor that initializes with a DbContext
        /// </summary>
        /// <param name="context">The database context to use for the repository actions</param>
        public EntityFrameworkRepository(DbContext context)
        {
            Context = context;
        }

        /// <inheritdoc />
        public virtual IQueryable<TEntity> Query => Context.Set<TEntity>();

        /// <inheritdoc />
        public virtual TEntity GetById(TIdentifier id) => Query.FirstOrDefault(e => e.Id.Equals(id));

        /// <inheritdoc />
        public virtual async Task<TEntity> GetByIdAsync(TIdentifier id) =>
            await Query.FirstOrDefaultAsync(e => e.Id.Equals(id));

        /// <inheritdoc />
        public virtual TEntity Create(TEntity entity)
        {
            var entityEntry = Context.Set<TEntity>().Add(entity);
            return entityEntry.Entity;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> CreateAsync(TEntity entity)
        {
            var entityEntry = await Context.Set<TEntity>().AddAsync(entity);
            return entityEntry.Entity;
        }

        /// <inheritdoc />
        public virtual List<TEntity> Create(ICollection<TEntity> entities)
        {
            Context.Set<TEntity>().AddRange(entities);
            return entities.ToList();
        }

        /// <inheritdoc />
        public virtual async Task<List<TEntity>> CreateAsync(ICollection<TEntity> entities)
        {
            await Context.Set<TEntity>().AddRangeAsync(entities);
            return entities.ToList();
        }

        /// <inheritdoc />
        public virtual TEntity Update(TEntity entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            return await Task.Run(() =>
            {
                Context.Entry(entity).State = EntityState.Modified;
                return entity;
            });
        }

        /// <inheritdoc />
        public virtual List<TEntity> Update(ICollection<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                Context.Entry(entity).State = EntityState.Modified;
            }

            return entities.ToList();
        }

        /// <inheritdoc />
        public virtual async Task<List<TEntity>> UpdateAsync(ICollection<TEntity> entities)
        {
            return await Task.Run(() =>
            {
                foreach (var entity in entities)
                {
                    Context.Entry(entity).State = EntityState.Modified;
                }

                return entities.ToList();
            });
        }

        /// <inheritdoc />
        public virtual TEntity Delete(TEntity entity) => Context.Remove(entity).Entity;

        /// <inheritdoc />
        public virtual async Task<TEntity> DeleteAsync(TEntity entity) =>
            await Task.Run(() => Context.Remove(entity).Entity);

        /// <inheritdoc />
        public virtual List<TEntity> Delete(ICollection<TEntity> entities)
        {
            Context.RemoveRange(entities);
            return entities.ToList();
        }

        /// <inheritdoc />
        public virtual async Task<List<TEntity>> DeleteAsync(ICollection<TEntity> entities)
        {
            return await Task.Run(() => Delete(entities));
        }

        /// <inheritdoc />
        public virtual async Task StartTransactionAsync()
        {
            if (Context.Database.CurrentTransaction == null)
                await Context.Database.BeginTransactionAsync();
        }

        /// <inheritdoc />
        public virtual async Task CommitTransactionAsync()
        {
            await Context.SaveChangesAsync();
            if (Context.Database.CurrentTransaction != null)
                Context.Database.CommitTransaction();
        }

        /// <inheritdoc />
        public virtual async Task RollbackTransactionAsync()
        {
            if (Context.Database.CurrentTransaction != null)
                await Task.Run(() => Context.Database.RollbackTransaction());
        }
    }
}