using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using CryptoTransactions.WinClient.Model.Entities;

namespace CryptoTransactions.WinClient.View.Windows
{
    public partial class WindowMain : Window
    {
        IEnumerable<Client> _clients;

        private static readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri("https://localhost:7002"),
            Timeout = TimeSpan.FromMinutes(3)
        };

        public WindowMain()
        {
            InitializeComponent();
            _clients = new List<Client>();
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
    }
}
