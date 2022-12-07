using System.Linq.Expressions;

namespace CryptoTransactions.API.Model.Repositories.Interfaces
{
    public interface IRepository<T> : IDisposable
    {
        Task<IEnumerable<T>> GetAllAsync();
        IEnumerable<T> GetByConditionAsync(Expression<Func<T, bool>> expression);
        void Create(T entity);
        void Delete(T entity);
        void Update(T entity);
        void SaveAsync();
    }
}
