using Newtonsoft.Json;
using System.Text;

namespace EnergyMonitor.MVC.Helpers
{
    public class ApiHelper
    {
        private readonly string _baseUrl;
        private readonly ILogger<ApiHelper> _logger;

        public ApiHelper(
            IConfiguration configuration,
            ILogger<ApiHelper> logger)
        {
            _baseUrl = configuration["ApiSettings:BaseUrl"]!;
            _logger = logger;
        }

        // GET request
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true;

                using var client = new HttpClient(handler);
                var response = await client.GetAsync(
                    $"{_baseUrl}{endpoint}");

                if (!response.IsSuccessStatusCode) return default;

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "GET {Endpoint} failed: {Error}",
                    endpoint, ex.Message);
                return default;
            }
        }

        // POST request
        public async Task<T?> PostAsync<T>(
            string endpoint, object data)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true;

                using var client = new HttpClient(handler);
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(
                    json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    $"{_baseUrl}{endpoint}", content);

                var result = await response.Content
                    .ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "POST {Endpoint} failed: {Error}",
                    endpoint, ex.Message);
                return default;
            }
        }

        // PUT request
        public async Task<T?> PutAsync<T>(
            string endpoint, object data)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true;

                using var client = new HttpClient(handler);
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(
                    json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(
                    $"{_baseUrl}{endpoint}", content);

                var result = await response.Content
                    .ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "PUT {Endpoint} failed: {Error}",
                    endpoint, ex.Message);
                return default;
            }
        }

        // DELETE request
        public async Task<T?> DeleteAsync<T>(string endpoint)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true;

                using var client = new HttpClient(handler);
                var response = await client.DeleteAsync(
                    $"{_baseUrl}{endpoint}");

                var result = await response.Content
                    .ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "DELETE {Endpoint} failed: {Error}",
                    endpoint, ex.Message);
                return default;
            }
        }
    }
}