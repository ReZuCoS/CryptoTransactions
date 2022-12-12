using CryptoTransactions.WinClient.Model;
using CryptoTransactions.WinClient.Model.Entities;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace CryptoTransactions.WinClient.View.Pages
{
    public partial class PageClientsList : Page
    {
        public PageClientsList()
        {
            InitializeComponent();

            try
            {
                LoadClientsList();
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

        private async void LoadClientsList()
        {
            listViewMain.ItemsSource = await WebApi.GetAll<Client>("/api/clients");
        }
    }
}
