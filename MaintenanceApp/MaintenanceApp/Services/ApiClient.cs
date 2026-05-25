using MaintenanceApp.Mapping.Response;
using MaintenanceApp.Singletone;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace MaintenanceApp.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient = new();

        public async Task<T?> GetAsync<T>(string path)
        {
            return await SendAsync<object, T>(HttpMethod.Get, path, null);
        }

        public async Task<T?> PostAsync<TRequest, T>(string path, TRequest body)
        {
            return await SendAsync<TRequest, T>(HttpMethod.Post, path, body);
        }

        public async Task<T?> PutAsync<TRequest, T>(string path, TRequest body)
        {
            return await SendAsync<TRequest, T>(HttpMethod.Put, path, body);
        }

        public async Task<T?> PatchAsync<TRequest, T>(string path, TRequest? body)
        {
            return await SendAsync<TRequest, T>(HttpMethod.Patch, path, body);
        }

        public async Task DeleteAsync(string path)
        {
            await SendAsync<object, string>(HttpMethod.Delete, path, null);
        }

        private async Task<TResponse?> SendAsync<TRequest, TResponse>(HttpMethod method, string path, TRequest? body)
        {
            var baseUrl = await GetBaseUrlAsync();
            var request = new HttpRequestMessage(method, $"{baseUrl}{path}");

            if (body != null)
            {
                var json = JsonConvert.SerializeObject(body);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default;
            }

            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<TResponse>>(content);
            if (!response.IsSuccessStatusCode || apiResponse == null || !apiResponse.Success)
            {
                throw new InvalidOperationException(apiResponse?.Message ?? $"API error: {(int)response.StatusCode}");
            }

            return apiResponse.Data;
        }

        private static async Task<string> GetBaseUrlAsync()
        {
            var connection = await SqliteDbContext.Instance.GetConnectionAsync();
            var baseUrl = connection?.Url?.Trim();

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = "http://10.0.2.2:120";
            }

            if (!baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                baseUrl = $"http://{baseUrl}";
            }

            return baseUrl.TrimEnd('/');
        }
    }
}
