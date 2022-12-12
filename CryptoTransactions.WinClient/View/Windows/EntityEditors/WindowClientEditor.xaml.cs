using CryptoTransactions.WinClient.Model;
using CryptoTransactions.WinClient.Model.Entities;
using System.Windows;

namespace CryptoTransactions.WinClient.View.Windows.EntityEditors
{
    public partial class WindowClientEditor : Window
    {
        private readonly Client _client;

        public WindowClientEditor(Client client)
        {
            InitializeComponent();

            _client = client;
            LoadClientData(_client);
        }

        private async void LoadClientData(Client client)
        {
            txtBoxWalletNumber.Text = client.WalletNumber;
            txtBoxSurname.Text = client.Surname;
            txtBoxName.Text = client.Name;
            txtBoxPatronymic.Text = client.Patronymic;
            txtBoxBalance.Text = client.Balance.ToString();

            listViewTransactions.ItemsSource =
                await WebApi.GetAll<Transaction>($"clients/{client.WalletNumber}/transactions");
        }

        private async void SaveChanges(object sender, RoutedEventArgs e)
        {
            _client.Surname = txtBoxSurname.Text;
            _client.Name = txtBoxName.Text;
            _client.Patronymic = txtBoxPatronymic.Text;
            _client.Balance = double.Parse(txtBoxBalance.Text);

            var responce = await WebApi.Update($"clients/{_client.WalletNumber}", _client);

            if (responce.IsSuccessStatusCode)
                this.DialogResult = true;
            else
                MessageBox.Show("Произошла ошибка при сохранении данных!");
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
