using CryptoTransactions.API.Model.Validators;
using MassTransit;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CryptoTransactions.API.Model.Entities
{
    /// <summary>
    /// System transactions
    /// </summary>
    public class Transaction
    {
        public Transaction() =>
            GenerateNewGUID();

        /// <summary>
        /// Transaction GUID
        /// </summary>
        [Key]
        [Required]
        public string GUID { get; private set; } = default!;

        /// <summary>
        /// Transaction date and time
        /// </summary>
        [Required]
        [DataType(DataType.DateTime)]
        public string TimeStamp { get; set; } = default!;

        /// <summary>
        /// GUID sender wallet number
        /// </summary>
        [Required]
        [GuidValue]
        [MinLength(36)]
        [MaxLength(36)]
        public string SenderWallet { get; set; } = default!;

        /// <summary>
        /// GUID recipient wallet number
        /// </summary>
        [Required]
        [GuidValue]
        [MinLength(36)]
        [MaxLength(36)]
        public string RecipientWallet { get; set; } = default!;

        /// <summary>
        /// Transaction amount
        /// </summary>
        [Required]
        [DefaultValue(0.0d)]
        [DataType(DataType.Currency)]
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

        public virtual Client? Sender { get; private set; }

        public virtual Client? Recipient { get; private set; }

        public void GenerateNewGUID() =>
            GUID = NewId.NextGuid().ToString().ToLower();

        public bool IsValid() =>
            !this.SenderWallet.Equals(this.RecipientWallet) &&
            this.Amount > 0;
    }
}
