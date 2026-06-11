using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using KalolCommunity.Application.Interfaces.Repositories;

namespace KalolCommunity.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public Repository(DbContext context)
        {
            _dbContext = context;
            _dbSet = _dbContext.Set<T>();
        }

        public async Task AddAsync(T entity)
            => await _dbSet.AddAsync(entity);

        public Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
                => _dbSet.AnyAsync(predicate);

        public Task<T?> GetAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = false)
            => asNoTracking
                ? _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate)
                : _dbSet.FirstOrDefaultAsync(predicate);
    }
}
