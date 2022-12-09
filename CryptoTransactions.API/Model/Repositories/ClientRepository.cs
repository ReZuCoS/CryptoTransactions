using CryptoTransactions.API.Model.Entities;
using CryptoTransactions.API.Model.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CryptoTransactions.API.Model.Repositories
{
    public class ClientRepository : IRepository<Client, string>, IDisposable
    {
        private readonly CryptoTransactionsContext _context;

        public ClientRepository(CryptoTransactionsContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Client>> GetAllAsync(Expression<Func<Client, bool>> expression)
        {
            return await _context.Clients
                .Where(expression)
                .ToListAsync();
        }

        public async Task<Client?> GetByKey(string key)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(c => c.WalletNumber.Equals(key));
        }

        public async Task<Client?> Find(string key)
        {
            return await _context.Clients
                .Where(c => c.WalletNumber.Equals(key))
                .Include(c => c.SentTransactions)
                .Include(c => c.ReceivedTransactions)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasAny(string key)
        {
            return await _context.Clients.AnyAsync(c => c.WalletNumber.Equals(key));
        }

        public async Task CreateAsync(Client client)
        {
            await _context.Clients.AddAsync(client);
        }

        public void Update(Client client)
        {
            _context.Entry(client).State = EntityState.Modified;
        }

        public void Delete(Client client)
        {
            _context.Clients.Remove(client);
        }

        public async Task SaveAsync()
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
