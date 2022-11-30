using CryptoTransactions.API.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace CryptoTransactions.API.Model
{
    public class CryptoTransactionsContext : DbContext
    {
        private static string? _connectionString;
        public static string? ConnectionString
        {
            get => _connectionString;
            set => _connectionString = !string.IsNullOrEmpty(value) ?
                value : throw new ArgumentNullException(nameof(value));
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlite(_connectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>()
                .HasMany(c => c.SentTransactions)
                .WithOne(t => t.Sender)
                .HasForeignKey(t => t.SenderWallet);

            modelBuilder.Entity<Client>()
                .HasMany(c => c.ReceivedTransactions)
                .WithOne(t => t.Recipient)
                .HasForeignKey(t => t.RecipientWallet);
        }
    }
}
