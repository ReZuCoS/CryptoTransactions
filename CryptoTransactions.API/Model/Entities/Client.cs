using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CryptoTransactions.API.Model.Entities
{
    /// <summary>
    /// System client
    /// </summary>
    [PrimaryKey(nameof(WalletNumber))]
    public class Client
    {
        /// <summary>
        /// Creates new client with GUID wallet number
        /// </summary>
        public Client()
        {
            WalletNumber = NewId.NextGuid().ToString();
            SentTransactions = new HashSet<Transaction>();
            ReceivedTransactions = new HashSet<Transaction>();
        }

        /// <summary>
        /// GUID client wallet nuber
        /// </summary>
        [Required]
        [MinLength(36)]
        [MaxLength(36)]
        public string? WalletNumber { get; private set; }

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
        public string? Patronymic { get; set; }

        /// <summary>
        /// Client's balance
        /// </summary>
        [DefaultValue(0.0d)]
        public double Balance { get; set; }

        [JsonIgnore]
        public virtual ICollection<Transaction>? SentTransactions { get; set; }

        [JsonIgnore]
        public virtual ICollection<Transaction>? ReceivedTransactions { get; set; }

        /// <summary>
        /// Generates new GUID for current client
        /// </summary>
        public void GenerateNewWalletNumber() =>
            WalletNumber = NewId.NextGuid().ToString();

        /// <summary>
        /// Updates client GUID
        /// </summary>
        /// <param name="walletNumber">GUID to update</param>
        public void UpdateWalletNumber(string walletNumber) =>
            this.WalletNumber = walletNumber;
    }
}
