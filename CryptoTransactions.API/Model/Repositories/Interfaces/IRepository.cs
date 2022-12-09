using System.Linq.Expressions;

namespace CryptoTransactions.API.Model.Repositories.Interfaces
{
    public interface IRepository<T, K> : IDisposable
        where T : class
        where K : class
    {
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> expression);
        Task<T?> GetByKey(K key);
        Task<T?> Find(K key);
        Task<bool> HasAny(K key);
        Task CreateAsync(T entity);
        void Delete(T entity);
        void Update(T entity);
        Task SaveAsync();
    }
}
