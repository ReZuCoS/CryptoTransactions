using System;

namespace CryptoTransactions.WinClient.Model.Entities
{
    public class Transaction
    {
        public Transaction()
        { }

        public Transaction(string senderWallet) : this()
        {
            SenderWallet = senderWallet;
        }
        public Transaction(string senderWallet, string recipientWallet) : this(senderWallet)
        {
            RecipientWallet = recipientWallet;
        }
        public Transaction(Client sender) : this()
        {
            SenderWallet = sender.WalletNumber;
        }
        public Transaction(Client sender, Client recipient) : this(sender)
        {
            RecipientWallet = recipient.WalletNumber;
        }
        public string? GUID { get; set; }
        public string? TimeStamp { get; set; }
        public string? SenderWallet { get; set; }
        public string? RecipientWallet { get; set; }
        public double Amount { get; set; }
        public string? CurrencyType { get; set; }
        public string? TransactionType { get; set; }
        public virtual Client Sender { get; set; }
        public virtual Client Recipient { get; set; }
        public DateTime DateTimeStamp =>
            DateTime.Parse(TimeStamp);
    }
}
