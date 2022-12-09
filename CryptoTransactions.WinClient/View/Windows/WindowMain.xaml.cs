using CryptoTransactions.WinClient.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using System.Threading;

namespace CryptoTransactions.WinClient.View.Windows
{
    public partial class WindowMain : Window
    {
        private static readonly HttpClient Client = new()
        {
            BaseAddress = new Uri("https://localhost:7002"),
            Timeout = TimeSpan.FromMinutes(3)
        };

        public WindowMain()
        {
            InitializeComponent();
        }

        private async void LoadClientsTable(object sender, RoutedEventArgs e)
        {
            try
            {
                listViewClients.ItemsSource = await GetClients();
            }
            catch (Exception ex)
            {
                MessageBox.Show("При выполнении программы произошла ошибка:\n" + ex.ToString(),
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<IEnumerable<Client>> GetClients()
        {
            var response = await Client.GetAsync("/api/clients");

            string content = await response
                .EnsureSuccessStatusCode()
                .Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Client>>(content);
        }
    }
}
