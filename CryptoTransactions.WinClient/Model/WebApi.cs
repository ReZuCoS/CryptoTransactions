using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace CryptoTransactions.WinClient.Model
{
    public static class WebApi
    {
        private static readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri("https://localhost:7002/api/"),
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

        public static async Task<HttpResponseMessage> Post(string uri, object entity)
        {
            var json = JsonConvert.SerializeObject(entity, Formatting.Indented);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            return await _httpClient.PostAsync(uri, data);
        }

        public static async Task<HttpResponseMessage> Update(string uri, object entity)
        {
            var json = JsonConvert.SerializeObject(entity, Formatting.Indented);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            return await _httpClient.PutAsync(uri, data);
        }

        public static async Task<HttpResponseMessage> Delete(string uri)
        {
            return await _httpClient.DeleteAsync(uri);
        }

        private static async Task<string> GetResponseContent(HttpResponseMessage response) =>
            await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
    }
}
