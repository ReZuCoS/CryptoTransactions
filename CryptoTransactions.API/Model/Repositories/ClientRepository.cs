using CryptoTransactions.API.Model.Entities;
using CryptoTransactions.API.Model.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CryptoTransactions.API.Model.Repositories
{
    public class ClientRepository : IRepository<Client>
    {
        private readonly CryptoTransactionsContext _context;

        public ClientRepository(CryptoTransactionsContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            return await _context.Clients.ToListAsync();
        }

        public IEnumerable<Client> GetByConditionAsync(Expression<Func<Client, bool>> expression)
        {
            return _context.Clients.Where(expression).ToList();
        }

        public void Create(Client client)
        {
            _context.Clients.Add(client);
        }

        public void Update(Client client)
        {
            _context.Entry(client).State = EntityState.Modified;
        }

        public void Delete(Client client)
        {
            _context.Clients.Remove(client);
        }

        public async void SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed = false;

        public void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                    _context.Dispose();

            _disposed = true;
        }
    }
}
