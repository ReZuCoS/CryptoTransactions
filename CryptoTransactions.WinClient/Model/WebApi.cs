using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CryptoTransactions.WinClient.Model
{
    public static class WebApi
    {
        private static readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri("https://localhost:7002"),
            Timeout = TimeSpan.FromMinutes(3)
        };

        public static async Task<IEnumerable<T>> GetAll<T>(string uri)
            where T : class
        {
            var response = await _httpClient.GetAsync(uri);
            return JsonConvert.DeserializeObject<List<T>>(await GetResponseContent(response));
        }

        public static async Task<T> Find<T>(string uri)
            where T : class
        {
            var response = await _httpClient.GetAsync(uri);
            return JsonConvert.DeserializeObject<T>(await GetResponseContent(response));
        }

        private async static Task<string> GetResponseContent(HttpResponseMessage response) =>
            await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
    }
}
