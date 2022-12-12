using CryptoTransactions.WinClient.Model.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace CryptoTransactions.WinClient.View.Windows
{
    public partial class WindowMain : Window
    {
        IEnumerable<Client> _clients;
        IEnumerable<Transaction> _transactions;

        private static readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri("https://localhost:7002"),
            Timeout = TimeSpan.FromMinutes(3)
        };

        public WindowMain()
        {
            InitializeComponent();
            _clients = new List<Client>();
            _transactions = new List<Transaction>();
        }

        private async void LoadClientsTable(object sender, RoutedEventArgs e)
        {
            try
            {
                _clients = await GetClients();
                listViewMain.ItemsSource = _clients;
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

        private static async Task<IEnumerable<Client>> GetClients()
        {
            var response = await _httpClient.GetAsync("/api/clients");

            string content = await response
                .EnsureSuccessStatusCode()
                .Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Client>>(content);
        }

        private async void LoadTransactionsTable(object sender, RoutedEventArgs e)
        {
            try
            {
                _transactions = await GetTransactions();
                listViewMain.ItemsSource = _transactions;
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

        private static async Task<IEnumerable<Transaction>> GetTransactions()
        {
            var response = await _httpClient.GetAsync("/api/transactions");

            string content = await response
                .EnsureSuccessStatusCode()
                .Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Transaction>>(content);
        }

        private void listViewMain_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
