namespace EnergyMonitor.Simulation
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly MqttPublisher _mqttPublisher;
        private readonly MachineLoader _machineLoader;
        private readonly int _intervalSeconds;
        private readonly int _factoryId;

        public Worker(
            ILogger<Worker> logger,
            IConfiguration configuration,
            MqttPublisher mqttPublisher,
            MachineLoader machineLoader)
        {
            _logger = logger;
            _configuration = configuration;
            _mqttPublisher = mqttPublisher;
            _machineLoader = machineLoader;
            _intervalSeconds = int.Parse(
                configuration["SimulationSettings:IntervalSeconds"]!);
            _factoryId = int.Parse(
                configuration["SimulationSettings:FactoryId"]!);
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "🚀 Energy Simulation Worker started!");

            // ✅ Load machines from DATABASE (not hardcoded!)
            var machines = await _machineLoader
                .LoadActiveMachinesAsync();

            if (!machines.Any())
            {
                _logger.LogWarning(
                    "⚠️ No active machines found in database!");
                return;
            }

            _logger.LogInformation(
                "✅ Simulating {Count} machines", machines.Count);

            // Connect to MQTT broker
            await _mqttPublisher.ConnectAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "⚡ Simulation cycle at {Time}",
                    DateTime.Now.ToString("HH:mm:ss"));

                foreach (var machine in machines)
                {
                    var reading = machine.GenerateReading();

                    await _mqttPublisher.PublishReadingAsync(
                        reading, _factoryId);

                    _logger.LogInformation(
                        "  🔌 {Machine} → {Power}KW | " +
                        "{Voltage}V | {Current}A",
                        machine.MachineName,
                        reading.PowerKW,
                        reading.VoltageV,
                        reading.CurrentA);
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(_intervalSeconds),
                    stoppingToken);
            }
        }
    }
}