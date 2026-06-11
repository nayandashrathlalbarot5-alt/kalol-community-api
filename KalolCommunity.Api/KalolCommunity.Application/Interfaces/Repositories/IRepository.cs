using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace KalolCommunity.Application.Interfaces.Repositories
{
    public interface IRepository<T> where T : class 
    {
        //Task<T> GetByIdAsync(Guid id);
        //Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate,bool asNoTracking = false);        
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        //void Remove(T entity);
    }
}
