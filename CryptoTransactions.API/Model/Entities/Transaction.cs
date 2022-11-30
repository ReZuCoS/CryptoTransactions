using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CryptoTransactions.API.Model.Entities
{
    [PrimaryKey(nameof(TimeStamp), nameof(SenderWallet), nameof(RecipientWallet))]
    public class Transaction
    {
        [Required]
        public string TimeStamp { get; set; } = default!;

        [Required]
        [MinLength(36)]
        [MaxLength(36)]
        public string SenderWallet { get; set; } = default!;

        [Required]
        [MinLength(36)]
        [MaxLength(36)]
        public string RecipientWallet { get; set; } = default!;

        [Required]
        [DefaultValue(0.0d)]
        public double Amount { get; set; }

        [Required]
        [MaxLength(75)]
        public string CurrencyType { get; set; } = default!;

        [Required]
        [MaxLength(75)]
        public string TransactionType { get; set; } = default!;

        [JsonIgnore]
        public virtual Client Sender { get; set; } = default!;

        [JsonIgnore]
        public virtual Client Recipient { get; set; } = default!;
    }
}
