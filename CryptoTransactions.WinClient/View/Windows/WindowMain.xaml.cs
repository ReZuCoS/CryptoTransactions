using CryptoTransactions.WinClient.Model;
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
        private readonly Uri _apiUri = new("https://localhost:7002");


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
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<IEnumerable<Client>> GetClients()
        {
            using var httpClient = new HttpClient()
            {
                BaseAddress = _apiUri,
                Timeout = TimeSpan.FromMinutes(3)
            };

            var response = await httpClient.GetAsync("/api/clients");

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Client>>(content);
        }
    }
}
