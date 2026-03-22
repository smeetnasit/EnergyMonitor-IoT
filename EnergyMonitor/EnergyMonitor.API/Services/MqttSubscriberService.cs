using EnergyMonitor.API.DTO;
using EnergyMonitor.API.Interface;
using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Text.Json;

namespace EnergyMonitor.API.Services
{
    public class MqttSubscriberService : BackgroundService
    {
        private readonly ILogger<MqttSubscriberService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private IMqttClient? _mqttClient;

        public MqttSubscriberService(
            ILogger<MqttSubscriberService> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "🔌 MQTT Subscriber Service starting...");

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            // ✅ What to do when message arrives
            _mqttClient.ApplicationMessageReceivedAsync +=
                OnMessageReceivedAsync;

            // ✅ Connect to Mosquitto
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(
                    _configuration["MqttSettings:BrokerHost"],
                    int.Parse(_configuration["MqttSettings:BrokerPort"]!))
                .WithClientId("energy-api-subscriber")
                .WithCleanSession()
                .Build();

            await _mqttClient.ConnectAsync(options, stoppingToken);
            _logger.LogInformation(
                "✅ Connected to MQTT Broker!");

            // ✅ Subscribe to all machine readings
            // factory/1/machine/+/energy
            // + means ANY machine ID
            await _mqttClient.SubscribeAsync(
                new MqttTopicFilterBuilder()
                    .WithTopic("factory/+/machine/+/energy")
                    .Build());

            _logger.LogInformation(
                "📡 Subscribed to: factory/+/machine/+/energy");

            // Keep running until app stops
            await Task.Delay(
                Timeout.Infinite, stoppingToken);
        }

        // ✅ Called every time a message arrives
        private async Task OnMessageReceivedAsync(
            MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                // Get the JSON payload
                var payload = Encoding.UTF8.GetString(
                    e.ApplicationMessage.PayloadSegment);

                _logger.LogInformation(
                    "📥 Received → Topic: {Topic}",
                    e.ApplicationMessage.Topic);

                // Deserialize JSON to object
                var reading = JsonSerializer.Deserialize<MqttReadingMessage>(
                    payload,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (reading == null) return;

                // Save to database using scoped service
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider
                    .GetRequiredService<IEnergyRepository>();

                var request = new InsertReadingRequest
                {
                    Machine_Id = reading.MachineId,
                    Power_KW = (decimal)reading.PowerKW,
                    Voltage_V = (decimal)reading.VoltageV,
                    Current_A = (decimal)reading.CurrentA,
                    PowerFactor = (decimal)reading.PowerFactor
                };

                // Simple anomaly detection
                // ✅ ML.NET Anomaly Detection
                var anomalyService = scope.ServiceProvider
                    .GetRequiredService<AnomalyDetectionService>();

                var anomalyResult = anomalyService.DetectAnomaly(
                    reading.MachineId,
                    (float)reading.PowerKW);

                bool isAnomaly = anomalyResult.IsAnomaly;
                decimal anomalyScore = anomalyResult.AnomalyScore;

                // If anomaly → save alert
                if (isAnomaly)
                {
                    await repository.InsertAlert(
                        reading.MachineId,
                        "PowerSpike",
                        $"Anomaly detected! Power: {reading.PowerKW}KW. {anomalyResult.Message}",
                        (decimal)reading.PowerKW);

                    _logger.LogWarning(
                        "🚨 ALERT created for Machine {MachineId}!",
                        reading.MachineId);
                }

                await repository.InsertReading(
                    request, isAnomaly, anomalyScore);

                _logger.LogInformation(
                    "💾 Saved → Machine {MachineId} | " +
                    "Power: {Power}KW",
                    reading.MachineId, reading.PowerKW);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "❌ Error processing message: {Error}",
                    ex.Message);
            }
        }

        public override async Task StopAsync(
            CancellationToken cancellationToken)
        {
            if (_mqttClient?.IsConnected == true)
                await _mqttClient.DisconnectAsync();
            await base.StopAsync(cancellationToken);
        }
    }

   
}