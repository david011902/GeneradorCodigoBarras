using GeneradorCodigoBarras.Models.DTOs;
using GeneradorCodigoBarras.Services.IServices;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;


namespace GeneradorCodigoBarras.Services
{
    internal class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(string baseUrl)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/")
            };
        }

        public void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<ProductResponseDto>> GetProductAsync()
        {
            var response = await _httpClient.GetAsync("api/products");

            if (!response.IsSuccessStatusCode)
                return new List<ProductResponseDto>();

            return await response.Content.ReadFromJsonAsync<List<ProductResponseDto>>(
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new List<ProductResponseDto>();
        }

        public async Task<ProductResponseDto?> GetProductByCodeAsync(string code)
        {
            var response = await _httpClient.GetAsync($"api/products/code/{Uri.EscapeDataString(code)}");

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<ProductResponseDto>();
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            var payload = JsonSerializer.Serialize(new { email, password });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/auth/login", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            foreach (var field in new[] { "token", "accessToken", "access_token", "jwt" })
                if (root.TryGetProperty(field, out var el))
                    return el.GetString();

            return null;
        }


    }
}
