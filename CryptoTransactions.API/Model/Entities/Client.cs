using MassTransit;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CryptoTransactions.API.Model.Entities
{
    /// <summary>
    /// System client
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Creates new client with GUID wallet number
        /// </summary>
        public Client()
        {
            GenerateNewWalletNumber();
            SentTransactions = new HashSet<Transaction>();
            ReceivedTransactions = new HashSet<Transaction>();
        }

        /// <summary>
        /// GUID client wallet number
        /// </summary>
        [Key]
        [Required]
        public string WalletNumber { get; private set; } = default!;

        /// <summary>
        /// Client's surname
        /// </summary>
        [Required]
        [MaxLength(50)]
        [DataType(DataType.Text)]
        public string Surname { get; set; } = default!;

        /// <summary>
        /// Client's name
        /// </summary>
        [Required]
        [MaxLength(50)]
        [DataType(DataType.Text)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Client's patronymic (if available)
        /// </summary>
        [MaxLength(50)]
        [DataType(DataType.Text)]
        public string Patronymic { get; set; } = string.Empty;

        /// <summary>
        /// Client's balance
        /// </summary>
        [DefaultValue(0.0d)]
        [DataType(DataType.Currency)]
        public double Balance { get; set; }

        [JsonIgnore]
        public virtual ICollection<Transaction> SentTransactions { get; private set; }

        [JsonIgnore]
        public virtual ICollection<Transaction> ReceivedTransactions { get; private set; }

        internal virtual ICollection<Transaction> Transactions =>
            SentTransactions.Union(ReceivedTransactions)
            .OrderBy(t => t.TimeStamp)
            .ToList();

        /// <summary>
        /// Generates new GUID for current client
        /// </summary>
        public void GenerateNewWalletNumber() =>
            WalletNumber = NewId.NextGuid().ToString().ToLower();

        public void TransferTo(Client client, double amount)
        {
            Balance -= amount <= Balance ? amount :
            throw new ArgumentOutOfRangeException(nameof(amount),
                "Value cannot be more than balance");

            client.Balance += amount;
        }

        /// <summary>
        /// Updates client GUID
        /// </summary>
        /// <param name="walletNumber">GUID to update</param>
        public void SetWalletNumber(string walletNumber) =>
            this.WalletNumber = walletNumber;
    }
}
