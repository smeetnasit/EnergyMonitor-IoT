using Dapper;

namespace EnergyMonitor.Simulation
{
    public class MachineLoader
    {
        private readonly DBContext _dbContext;
        private readonly ILogger<MachineLoader> _logger;

        public MachineLoader(
            DBContext dbContext,
            ILogger<MachineLoader> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<MachineSimulator>> LoadActiveMachinesAsync()
        {
            using var connection = _dbContext.CreateConnection();

            var machines = await connection.QueryAsync<MachineSimulator>(
                @"SELECT 
                    Machine_Id   AS MachineId,
                    Machine_Name AS MachineName,
                    Machine_Type AS MachineType,
                    Max_Power_KW AS MaxPowerKW
                  FROM Machines
                  WHERE IsActive = 1
                  ORDER BY Machine_Id");

            var result = machines.ToList();

            // Set BaseLoadPercent based on machine type
            foreach (var machine in result)
            {
                machine.BaseLoadPercent = machine.MachineType switch
                {
                    "Conveyor" => 0.70,
                    "CNC" => 0.80,
                    "Compressor" => 0.65,
                    "Pump" => 0.75,
                    _ => 0.70  // default 70%
                };
            }

            _logger.LogInformation(
                "✅ Loaded {Count} active machines from database",
                result.Count);

            return result;
        }
    }
}