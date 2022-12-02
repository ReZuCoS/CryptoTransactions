namespace CryptoTransactions.API.Model.Entities.QueryEntities
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

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(TimeStamp) &&
                string.IsNullOrEmpty(SenderWallet) &&
                string.IsNullOrEmpty(RecipientWallet);
        }
    }
}
