using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Text.Json;

namespace EnergyMonitor.Simulation
{
    public class MqttPublisher : IAsyncDisposable
    {
        private readonly IMqttClient _mqttClient;
        private readonly ILogger<MqttPublisher> _logger;
        private readonly string _brokerHost;
        private readonly int _brokerPort;
        private readonly string _clientId;

        public MqttPublisher(
            IConfiguration configuration,
            ILogger<MqttPublisher> logger)
        {
            _logger = logger;
            _brokerHost = configuration["MqttSettings:BrokerHost"]!;
            _brokerPort = int.Parse(
                configuration["MqttSettings:BrokerPort"]!);
            _clientId = configuration["MqttSettings:ClientId"]!;

            // Create MQTT client
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
        }

        // Connect to Mosquitto broker
        public async Task ConnectAsync()
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_brokerHost, _brokerPort)
                .WithClientId(_clientId)
                .WithCleanSession()
                .Build();

            await _mqttClient.ConnectAsync(options);
            _logger.LogInformation(
                "✅ Connected to MQTT Broker at {Host}:{Port}",
                _brokerHost, _brokerPort);
        }

        // Publish one reading to MQTT topic
        public async Task PublishReadingAsync(
            SimulatedReading reading, int factoryId)
        {
            // Create topic: factory/1/machine/1/energy
            var topic = $"factory/{factoryId}/machine/" +
                        $"{reading.MachineId}/energy";

            // Convert reading to JSON
            var payload = JsonSerializer.Serialize(new
            {
                machineId = reading.MachineId,
                powerKW = reading.PowerKW,
                voltageV = reading.VoltageV,
                currentA = reading.CurrentA,
                powerFactor = reading.PowerFactor,
                timestamp = reading.Timestamp
            });

            // Create MQTT message
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .WithQualityOfServiceLevel(
                    MQTTnet.Protocol.MqttQualityOfServiceLevel
                    .AtLeastOnce)
                .Build();

            // Send to broker
            await _mqttClient.PublishAsync(message);

            _logger.LogInformation(
                "📤 Published → Topic: {Topic} | " +
                "Power: {Power}KW",
                topic, reading.PowerKW);
        }

        public bool IsConnected => _mqttClient.IsConnected;

        public async ValueTask DisposeAsync()
        {
            if (_mqttClient.IsConnected)
                await _mqttClient.DisconnectAsync();
            _mqttClient.Dispose();
        }
    }
}



//### Explanation — What We Just Built 🧠

//**MQTT Client Options:**
//```
//WithTcpServer("localhost", 1883)
//→ connect to Mosquitto running in Docker on port 1883

//WithClientId("energy-simulator")
//→ give our publisher a unique name
//→ Mosquitto tracks connected clients by ID

//WithCleanSession()
//→ start fresh every time we connect
//→ don't remember old messages
//```

//**Topic Structure:**
//```
//factory / 1 / machine / 1 / energy

//factory  → top level category
//1        → factory ID
//machine  → sub category
//1        → machine ID
//energy   → type of data

//Why this structure?
//API can subscribe to:
//factory / 1 / machine / +/ energy  → all machines
//factory/#                   → everything
//```

//**QoS (Quality of Service):**
//```
//QoS Level 0 → Fire and forget (no guarantee)
//QoS Level 1 → At least once (guaranteed delivery) ← we use this
//QoS Level 2 → Exactly once (no duplicates, slowest)

//We use Level 1 → important data must arrive!