using CryptoTransactions.API.Model.Entities;
using CryptoTransactions.API.Model.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CryptoTransactions.API.Model.Repositories
{
    public class TransactionRepository : IRepository<Transaction, string>, IDisposable
    {
        private readonly CryptoTransactionsContext _context;

        public TransactionRepository(CryptoTransactionsContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync(Expression<Func<Transaction, bool>> expression)
        {
            return await _context.Transactions
                .Where(expression)
                .ToListAsync();
        }

        public async Task<Transaction?> GetByKey(string key)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(c => c.GUID.Equals(key));
        }

        public async Task<Transaction?> GetByKeyDetailedAsync(string key)
        {
            return await _context.Transactions
                .Where(t => t.GUID.Equals(key))
                .Include(t => t.Sender)
                .Include(t => t.Recipient)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasAny(string key)
        {
            return await _context.Transactions.AnyAsync(c => c.GUID.Equals(key));
        }

        public async Task CreateAsync(Transaction transaction)
        {
            var sender = await _context.Clients.FirstOrDefaultAsync(c =>
                c.WalletNumber == transaction.SenderWallet);
            var recipient = await _context.Clients.FirstOrDefaultAsync(c =>
                c.WalletNumber == transaction.RecipientWallet);

            if (sender is null)
                throw new ArgumentException("Sender wallet not found!");
            if (recipient is null)
                throw new ArgumentException("Recipient wallet not found!");

            sender.DecreaseBalance(transaction.Amount);
            _context.Entry(sender).State = EntityState.Modified;

            recipient.ReplenishBalance(transaction.Amount);
            _context.Entry(recipient).State = EntityState.Modified;

            _context.Transactions.Add(transaction);
        }

        public void Update(Transaction transaction)
        {
            _context.Entry(transaction).State = EntityState.Modified;
        }

        public void Delete(Transaction transaction)
        {
            _context.Transactions.Remove(transaction);
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
