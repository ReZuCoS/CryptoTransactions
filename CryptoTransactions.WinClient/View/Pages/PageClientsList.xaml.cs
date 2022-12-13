using CryptoTransactions.WinClient.Model;
using CryptoTransactions.WinClient.Model.Entities;
using CryptoTransactions.WinClient.View.Windows.EntityEditors;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CryptoTransactions.WinClient.View.Pages
{
    public partial class PageClientsList : Page
    {
        public PageClientsList()
        {
            InitializeComponent();

            cboxLimit.SelectionChanged += UpdateRowsCount;
        }

        private async void LoadClientsList(object sender, RoutedEventArgs e)
        {
            try
            {
                listViewMain.ItemsSource = await GetClientsList();
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

        private async Task<IEnumerable<Client>> GetClientsList()
        {
            return await WebApi.GetAll<Client>(GenerateUri());
        }

        private string GenerateUri()
        {
            int pageNumber = Convert.ToInt32(txtBoxPageNumber.Text);
            int limit = Convert.ToInt32(((ComboBoxItem)cboxLimit.SelectedItem).Content);
            int offset = limit * (pageNumber - 1);

            var uriBuilder = new StringBuilder("clients?");

            if (IsFilteredSearch())
            {
                string surname = txtBoxSurname.Text;
                string name = txtBoxName.Text;
                string patronymic = txtBoxPathronymic.Text;

                if (!string.IsNullOrEmpty(surname))
                    uriBuilder.Append($"&Surname={surname}");

                if (!string.IsNullOrEmpty(name))
                    uriBuilder.Append($"&Name={name}");

                if (!string.IsNullOrEmpty(patronymic))
                    uriBuilder.Append($"&Patronymic={patronymic}");
            }

            uriBuilder.Append($"&limit={limit}&offset={offset}");

            return uriBuilder.ToString();
        }

        private bool IsFilteredSearch() =>
            !string.IsNullOrWhiteSpace(txtBoxSurname.Text) ||
                !string.IsNullOrWhiteSpace(txtBoxName.Text) ||
                !string.IsNullOrWhiteSpace(txtBoxPathronymic.Text);

        private async void AddNewClient(object sender, RoutedEventArgs e)
        {
            var isUpdateRequired = new WindowClientEditor()
                .ShowDialog();

            if (isUpdateRequired.HasValue && isUpdateRequired.Value)
                listViewMain.ItemsSource = await GetClientsList();
        }

        private async void EditSelectedClient(object sender, MouseButtonEventArgs e)
        {
            var selectedClient = (Client)listViewMain.SelectedItem;

            var isUpdateRequired = new WindowClientEditor(selectedClient)
                .ShowDialog();

            if (isUpdateRequired.HasValue && isUpdateRequired.Value)
                listViewMain.ItemsSource = await GetClientsList();
        }

        private async void UpdateRowsCount(object sender, SelectionChangedEventArgs e)
        {
            listViewMain.ItemsSource = await GetClientsList();
        }

        private async void GoPreviousPage(object sender, RoutedEventArgs e)
        {
            var currentNumber = int.Parse(txtBoxPageNumber.Text);
            txtBoxPageNumber.Text = (currentNumber - 1).ToString();
            listViewMain.ItemsSource = await GetClientsList();
        }

        private async void GoNextPage(object sender, RoutedEventArgs e)
        {
            var currentNumber = int.Parse(txtBoxPageNumber.Text);
            txtBoxPageNumber.Text = (currentNumber + 1).ToString();
            listViewMain.ItemsSource = await GetClientsList();
        }

        private async void UpdateListOnTextChange(object sender, TextChangedEventArgs e)
        {
            listViewMain.ItemsSource = await GetClientsList();
        }
    }
}
