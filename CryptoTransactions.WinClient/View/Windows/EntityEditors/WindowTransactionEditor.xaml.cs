using CryptoTransactions.WinClient.Model.Entities;
using System.Windows;
using System.Windows.Controls;

namespace CryptoTransactions.WinClient.View.Windows.EntityEditors
{
    public partial class WindowTransactionEditor : Window
    {
        private readonly Transaction _transaction;

        public WindowTransactionEditor()
        {
            InitializeComponent();
            _transaction = new Transaction();
            LoadTransactionData(_transaction);
        }

        public WindowTransactionEditor(Client sender) : this()
        {
            _transaction = new Transaction(sender);
        }

        private void LoadTransactionData(Transaction transaction)
        {
            DisableFieldIfPresented(txtBoxSenderWallet, transaction.SenderWallet);
            DisableFieldIfPresented(txtBoxRecipientWallet, transaction.RecipientWallet);
        }

        private static void DisableFieldIfPresented(TextBox textBox, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                textBox.Text = value;
                textBox.IsEnabled = false;
            }
        }
    }
}
