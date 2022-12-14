using CryptoTransactions.WinClient.Model;
using CryptoTransactions.WinClient.Model.Entities;
using System.Windows;

namespace CryptoTransactions.WinClient.View.Windows.EntityEditors
{
    public partial class WindowClientEditor : Window
    {
        private readonly Client _client;

        public WindowClientEditor()
        {
            InitializeComponent();
            _client = new();
        }

        public WindowClientEditor(Client client) : this()
        {
            this.Height = 675;
            this.Title = "Изменение клиента";

            _client = client;
            LoadClientData(_client);
        }

        private async void LoadClientData(Client client)
        {
            txtBoxWalletNumber.Text = client.WalletNumber;
            txtBoxSurname.Text = client.Surname;
            txtBoxName.Text = client.Name;
            txtBoxPatronymic.Text = client.Patronymic;
            txtBoxBalance.Text = $"{client.Balance.ToString():00}";

            if (!string.IsNullOrEmpty(client.WalletNumber))
                listViewTransactions.ItemsSource =
                    await WebApi.GetAll<Transaction>($"clients/{client.WalletNumber}/transactions");
        }

        private async void SaveChanges(object sender, RoutedEventArgs e)
        {
            _client.Surname = txtBoxSurname.Text;
            _client.Name = txtBoxName.Text;
            _client.Patronymic = txtBoxPatronymic.Text;
            _client.Balance = double.Parse(txtBoxBalance.Text);

            var response = string.IsNullOrEmpty(_client.WalletNumber)
                ? await WebApi.Post($"clients", _client)
                : await WebApi.Update($"clients/{_client.WalletNumber}", _client);

            if (response.IsSuccessStatusCode)
                this.DialogResult = true;
        }

        private async void RemoveClient(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show($"Вы действительно хотите удалить клиента:\n" +
                $"{_client.Surname} {_client.Name}?\n" +
                $"Отменить данное действие невозможно!",
                "Подтвердите удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            var response = await WebApi.Delete($"clients/{_client.WalletNumber}");

            if (response.IsSuccessStatusCode)
                this.DialogResult = true;
        }

        private void AddTransaction(object sender, RoutedEventArgs e)
        {
            var isSuccess = new WindowTransactionEditor(_client)
                .ShowDialog();

        }

        private async void DeleteTransaction(object sender, RoutedEventArgs e)
        {
            var selectedTransaction = (Transaction)listViewTransactions.SelectedItem;

            if (selectedTransaction is null)
                return;

            var messageBoxResult = MessageBox.Show("Вы действительно хотите удалить транзакцию\n" +
                $"{selectedTransaction.GUID}?\n" +
                $"Данное действие невозможно отменить!", "Подтвердите действие",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.No)
                return;

            var response = await WebApi.Delete($"transactions/{selectedTransaction.GUID}");

            if (response.IsSuccessStatusCode)
                listViewTransactions.ItemsSource =
                    await WebApi.GetAll<Transaction>($"clients/{_client.WalletNumber}/transactions");
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
