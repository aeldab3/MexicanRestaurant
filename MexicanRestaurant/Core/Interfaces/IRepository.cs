using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using System.Linq.Expressions;

namespace MexicanRestaurant.Core.Interfaces
{
    public interface IRepository <T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(QueryOptions<T> options);
        Task<IEnumerable<T>> GetAllByIdAsync<TKey>(TKey id, string propertyName, QueryOptions<T> options);
        Task<T> GetByIdAsync(int id, QueryOptions<T> options);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        IQueryable<T> Table { get; }
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }
}
