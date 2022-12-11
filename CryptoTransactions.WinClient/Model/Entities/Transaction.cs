namespace CryptoTransactions.WinClient.Model.Entities
{
    public class Transaction
    {
        public Transaction() { }
        public string GUID { get; set; }
        public string TimeStamp { get; set; }
        public string SenderWallet { get; set; }
        public string RecipientWallet { get; set; }
        public double Amount { get; set; }
        public string CurrencyType { get; set; }
        public string TransactionType { get; set; }
        public virtual Client Sender { get; set; }
        public virtual Client Recipient { get; set; }
    }
}
