using System.Xml.Linq;

namespace CryptoTransactions.API.Model.Entities
{
    /// <summary>
    /// System transactions
    /// </summary>
    public class TransactionQuery
    {
        /// <summary>
        /// Transaction date and time
        /// </summary>
        public string TimeStamp { get; set; } = string.Empty;

        /// <summary>
        /// GUID sender wallet number
        /// </summary>
        public string SenderWallet { get; set; } = string.Empty;

        /// <summary>
        /// GUID recipient wallet number
        /// </summary>
        public string RecipientWallet { get; set; } = string.Empty;

        /// <summary>
        /// Currency type
        /// </summary>
        public string CurrencyType { get; set; } = string.Empty;

        /// <summary>
        /// Transaction type
        /// </summary>
        public string TransactionType { get; set; } = string.Empty;

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(TimeStamp) &&
                string.IsNullOrEmpty(SenderWallet) &&
                string.IsNullOrEmpty(RecipientWallet) &&
                string.IsNullOrEmpty(CurrencyType) &&
                string.IsNullOrEmpty(TransactionType);
        }
    }
}
