using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CryptoTransactions.API.Model.Entities
{
    /// <summary>
    /// System transactions
    /// </summary>
    [PrimaryKey(nameof(TimeStamp), nameof(SenderWallet), nameof(RecipientWallet))]
    public class Transaction
    {
        /// <summary>
        /// Transaction date and time
        /// </summary>
        [Required]
        public string TimeStamp { get; set; } = default!;

        /// <summary>
        /// GUID sender wallet number
        /// </summary>
        [Required]
        [MinLength(36)]
        [MaxLength(36)]
        public string SenderWallet { get; set; } = default!;

        /// <summary>
        /// GUID recipient wallet number
        /// </summary>
        [Required]
        [MinLength(36)]
        [MaxLength(36)]
        public string RecipientWallet { get; set; } = default!;

        /// <summary>
        /// Transaction amount
        /// </summary>
        [Required]
        [DefaultValue(0.0d)]
        public double Amount { get; set; }

        /// <summary>
        /// Currency type
        /// </summary>
        [Required]
        [MaxLength(75)]
        public string CurrencyType { get; set; } = default!;

        /// <summary>
        /// Transaction type
        /// </summary>
        [Required]
        [MaxLength(75)]
        public string TransactionType { get; set; } = default!;

        [JsonIgnore]
        public virtual Client Sender { get; set; } = default!;

        [JsonIgnore]
        public virtual Client Recipient { get; set; } = default!;
    }
}
