using EnergyMonitor.API.DTO;
using System.Diagnostics.Contracts;

namespace EnergyMonitor.API.Interface
{
    public interface IEnergyRepository
    {
        // Factory
        Task<List<FactoryDTO>> GetAllFactories();

        // Machine
        Task<List<MachineDTO>> GetAllMachines(int factoryId);
        Task UpsertMachine(UpsertMachineRequest request);
        Task DeleteMachine(int machineId);

        // Energy Readings
        Task InsertReading(InsertReadingRequest request, bool isAnomaly, decimal anomalyScore);
        Task<List<EnergyReadingDTO>> GetLatestReadings(int factoryId);
        Task<List<EnergyReadingDTO>> GetReadingHistory(int machineId, int hours);

        // Alerts
        Task<List<AlertDTO>> GetAlerts(int factoryId);
        Task InsertAlert(int machineId, string alertType, string message, decimal powerValue);
        Task UpdateAlertStatus(int alertId, string status);

        // Cost Config
        Task<EnergyCostConfigDTO?> GetCostConfig(int factoryId);
        Task<UserDTO?> GetUserByEmail(string email);
    }
}
