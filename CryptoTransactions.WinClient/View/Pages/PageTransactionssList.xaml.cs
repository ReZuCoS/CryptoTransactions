using CryptoTransactions.WinClient.Model;
using CryptoTransactions.WinClient.Model.Entities;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace CryptoTransactions.WinClient.View.Pages
{
    public partial class PageTransactionsList : Page
    {
        public PageTransactionsList()
        {
            InitializeComponent();

            try
            {
                LoadTransactionsList();
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

        private async void LoadTransactionsList()
        {
            listViewMain.ItemsSource = await WebApi.GetAll<Transaction>("/api/transactions");
        }
    }
}
