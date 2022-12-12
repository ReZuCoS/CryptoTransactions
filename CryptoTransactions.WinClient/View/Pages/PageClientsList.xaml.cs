using CryptoTransactions.WinClient.Model;
using CryptoTransactions.WinClient.Model.Entities;
using CryptoTransactions.WinClient.View.Windows.EntityEditors;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CryptoTransactions.WinClient.View.Pages
{
    public partial class PageClientsList : Page
    {
        public PageClientsList() =>
            InitializeComponent();

        private async void LoadClientsList(object sender, RoutedEventArgs e)
        {
            try
            {
                listViewMain.ItemsSource = await WebApi.GetAll<Client>("clients");
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

        private void EditSelectedClient(object sender, MouseButtonEventArgs e)
        {
            var selectedClient = (Client)listViewMain.SelectedItem;

            var isSuccess = new WindowClientEditor(selectedClient)
                .ShowDialog();

            if (isSuccess.Value)
                LoadClientsList(default, default);
        }
    }
}
