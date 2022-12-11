using System.Collections.Generic;
using System.Linq;

namespace CryptoTransactions.WinClient.Model.Entities
{
    public class Client
    {
        public Client()
        {
            SentTransactions = new HashSet<Transaction>();
            ReceivedTransactions = new HashSet<Transaction>();
        }

        public string WalletNumber { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public double Balance { get; set; }
        public virtual ICollection<Transaction> SentTransactions { get; set; }
        public virtual ICollection<Transaction> ReceivedTransactions { get; set; }
        internal virtual ICollection<Transaction> Transactions =>
            SentTransactions.Union(ReceivedTransactions)
            .OrderBy(t => t.TimeStamp)
            .ToList();
    }
}
