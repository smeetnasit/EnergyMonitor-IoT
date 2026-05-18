using Newtonsoft.Json;

namespace EnergyMonitor.MVC.Helpers
{
    public class ApiHelper : IApiHelper
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiHelper> _logger;
        private readonly IConfiguration _configuration;

        public ApiHelper(HttpClient httpClient, ILogger<ApiHelper> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;

            // Set base address from configuration
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5000";
            _httpClient.BaseAddress = new Uri(apiBaseUrl);
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var jsonContent = JsonConvert.SerializeObject(data);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation($"POST request to {endpoint}");

                var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(responseContent);
                }
                else
                {
                    _logger.LogError($"POST request failed with status code {response.StatusCode}");
                    return default;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during POST request to {endpoint}: {ex.Message}");
                return default;
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                _logger.LogInformation($"GET request to {endpoint}");

                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(responseContent);
                }
                else
                {
                    _logger.LogError($"GET request failed with status code {response.StatusCode}");
                    return default;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during GET request to {endpoint}: {ex.Message}");
                return default;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                _logger.LogInformation($"DELETE request to {endpoint}");

                var response = await _httpClient.DeleteAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"DELETE request successful");
                    return true;
                }
                else
                {
                    _logger.LogError($"DELETE request failed with status code {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during DELETE request to {endpoint}: {ex.Message}");
                return false;
            }
        }
    }
}
