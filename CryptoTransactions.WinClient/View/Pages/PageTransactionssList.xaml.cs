using CryptoTransactions.WinClient.Model;
using CryptoTransactions.WinClient.Model.Entities;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CryptoTransactions.WinClient.View.Pages
{
    public partial class PageTransactionsList : Page
    {
        public PageTransactionsList() =>
            InitializeComponent();

        private async void LoadTransactionsList(object sender, RoutedEventArgs e)
        {
            try
            {
                listViewMain.ItemsSource = await WebApi.GetAll<Transaction>("/api/transactions");
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Ошибка при подключению к серверу!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("При выполнении программы произошла ошибка:\n" + ex.ToString(),
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditSelectedTransaction(object sender, MouseButtonEventArgs e)
        {
            var transaction = (Transaction)listViewMain.SelectedItem;


        }
    }
}
