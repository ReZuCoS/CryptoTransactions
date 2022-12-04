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
        public string Surname { get; set; } = default!;

        /// <summary>
        /// Client's name
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Client's patronymic (if available)
        /// </summary>
        [MaxLength(50)]
        public string Patronymic { get; set; } = string.Empty;

        /// <summary>
        /// Client's balance
        /// </summary>
        [DefaultValue(0.0d)]
        public double Balance { get;  private set; }

        [JsonIgnore]
        public virtual ICollection<Transaction> SentTransactions { get; private set; }

        [JsonIgnore]
        public virtual ICollection<Transaction> ReceivedTransactions { get; private set; }
        
        /// <summary>
        /// Generates new GUID for current client
        /// </summary>
        public void GenerateNewWalletNumber() =>
            WalletNumber = NewId.NextGuid().ToString().ToLower();

        public void ReplenishBalance(double count) =>
            Balance += count;

        public void DecreaseBalance(double count) =>
            Balance -= count <= Balance ?
            count :
            throw new ArgumentOutOfRangeException(
                $"{nameof(count)} cannot be more than balance");

        /// <summary>
        /// Updates client GUID
        /// </summary>
        /// <param name="walletNumber">GUID to update</param>
        public void SetWalletNumber(string walletNumber) =>
            this.WalletNumber = walletNumber;
    }
}
