namespace EnergyMonitor.MVC.Helpers
{
    public interface IApiHelper
    {
        Task<T?> PostAsync<T>(string endpoint, object data);
        Task<T?> GetAsync<T>(string endpoint);
        Task<bool> DeleteAsync(string endpoint);
    }
}
