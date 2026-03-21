using Dapper;
using EnergyMonitor.API.DTO;
using EnergyMonitor.API.Interface;
using System.Data;

namespace EnergyMonitor.API.Repository
{
    public class EnergyRepository : IEnergyRepository
    {
        private readonly DBContext _dbContext;

        public EnergyRepository(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ─── FACTORIES ───────────────────────────────────────
        public async Task<List<FactoryDTO>> GetAllFactories()
        {
            using var connection = _dbContext.CreateConnection();
            var result = await connection.QueryAsync<FactoryDTO>(
                "SELECT * FROM Factories WHERE IsActive = 1");
            return result.ToList();
        }

        // ─── MACHINES ────────────────────────────────────────
        public async Task<List<MachineDTO>> GetAllMachines(int factoryId)
        {
            using var connection = _dbContext.CreateConnection();
            var result = await connection.QueryAsync<MachineDTO>(
                @"SELECT m.*, f.Factory_Name 
                  FROM Machines m
                  INNER JOIN Factories f ON m.Factory_Id = f.Factory_Id
                  WHERE m.Factory_Id = @FactoryId 
                  AND m.IsActive = 1",
                new { FactoryId = factoryId });
            return result.ToList();
        }

        public async Task UpsertMachine(UpsertMachineRequest request)
        {
            using var connection = _dbContext.CreateConnection();
            if (request.Machine_Id == 0)
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO Machines 
                      (Factory_Id, Machine_Name, Machine_Type, Max_Power_KW)
                      VALUES (@Factory_Id, @Machine_Name, 
                              @Machine_Type, @Max_Power_KW)",
                    request);
            }
            else
            {
                await connection.ExecuteAsync(
                    @"UPDATE Machines SET
                      Machine_Name = @Machine_Name,
                      Machine_Type = @Machine_Type,
                      Max_Power_KW = @Max_Power_KW
                      WHERE Machine_Id = @Machine_Id",
                    request);
            }
        }

        public async Task DeleteMachine(int machineId)
        {
            using var connection = _dbContext.CreateConnection();
            await connection.ExecuteAsync(
                "UPDATE Machines SET IsActive = 0 WHERE Machine_Id = @Id",
                new { Id = machineId });
        }

        // ─── ENERGY READINGS ─────────────────────────────────
        public async Task InsertReading(InsertReadingRequest request,
            bool isAnomaly, decimal anomalyScore)
        {
            using var connection = _dbContext.CreateConnection();
            await connection.ExecuteAsync(
                @"INSERT INTO EnergyReadings
                  (Machine_Id, Power_KW, Voltage_V, Current_A, 
                   PowerFactor, Is_Anomaly, Anomaly_Score)
                  VALUES (@Machine_Id, @Power_KW, @Voltage_V, 
                          @Current_A, @PowerFactor, 
                          @IsAnomaly, @AnomalyScore)",
                new
                {
                    request.Machine_Id,
                    request.Power_KW,
                    request.Voltage_V,
                    request.Current_A,
                    request.PowerFactor,
                    IsAnomaly = isAnomaly,
                    AnomalyScore = anomalyScore
                });
        }

        public async Task<List<EnergyReadingDTO>> GetLatestReadings(
            int factoryId)
        {
            using var connection = _dbContext.CreateConnection();
            var result = await connection.QueryAsync<EnergyReadingDTO>(
                @"SELECT TOP 1 WITH TIES
                    er.*, m.Machine_Name, m.Machine_Type
                  FROM EnergyReadings er
                  INNER JOIN Machines m ON er.Machine_Id = m.Machine_Id
                  WHERE m.Factory_Id = @FactoryId
                  AND m.IsActive = 1
                  ORDER BY ROW_NUMBER() OVER 
                    (PARTITION BY er.Machine_Id 
                     ORDER BY er.Reading_Time DESC)",
                new { FactoryId = factoryId });
            return result.ToList();
        }

        public async Task<List<EnergyReadingDTO>> GetReadingHistory(
            int machineId, int hours)
        {
            using var connection = _dbContext.CreateConnection();
            var result = await connection.QueryAsync<EnergyReadingDTO>(
                @"SELECT er.*, m.Machine_Name, m.Machine_Type
                  FROM EnergyReadings er
                  INNER JOIN Machines m ON er.Machine_Id = m.Machine_Id
                  WHERE er.Machine_Id = @MachineId
                  AND er.Reading_Time >= DATEADD(HOUR, -@Hours, GETDATE())
                  ORDER BY er.Reading_Time ASC",
                new { MachineId = machineId, Hours = hours });
            return result.ToList();
        }

        // ─── ALERTS ──────────────────────────────────────────
        public async Task<List<AlertDTO>> GetAlerts(int factoryId)
        {
            using var connection = _dbContext.CreateConnection();
            var result = await connection.QueryAsync<AlertDTO>(
                @"SELECT TOP 100 a.*, m.Machine_Name
                  FROM Alerts a
                  INNER JOIN Machines m ON a.Machine_Id = m.Machine_Id
                  WHERE m.Factory_Id = @FactoryId
                  ORDER BY a.Created_Date DESC",
                new { FactoryId = factoryId });
            return result.ToList();
        }

        public async Task InsertAlert(int machineId, string alertType,
            string message, decimal powerValue)
        {
            using var connection = _dbContext.CreateConnection();
            await connection.ExecuteAsync(
                @"INSERT INTO Alerts 
                  (Machine_Id, Alert_Type, Alert_Message, Power_Value)
                  VALUES (@MachineId, @AlertType, @Message, @PowerValue)",
                new
                {
                    MachineId = machineId,
                    AlertType = alertType,
                    Message = message,
                    PowerValue = powerValue
                });
        }

        public async Task UpdateAlertStatus(int alertId, string status)
        {
            using var connection = _dbContext.CreateConnection();
            await connection.ExecuteAsync(
                @"UPDATE Alerts SET Alert_Status = @Status 
                  WHERE Alert_Id = @AlertId",
                new { AlertId = alertId, Status = status });
        }

        // ─── COST CONFIG ─────────────────────────────────────
        public async Task<EnergyCostConfigDTO?> GetCostConfig(int factoryId)
        {
            using var connection = _dbContext.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<EnergyCostConfigDTO>(
                @"SELECT TOP 1 * FROM EnergyCostConfig
                  WHERE Factory_Id = @FactoryId
                  ORDER BY Valid_From DESC",
                new { FactoryId = factoryId });
        }
    }
}
