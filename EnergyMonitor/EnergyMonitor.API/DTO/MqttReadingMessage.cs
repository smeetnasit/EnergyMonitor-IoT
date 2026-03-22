namespace EnergyMonitor.API.DTO
{
    // DTO for incoming MQTT message
    public class MqttReadingMessage
    {
        public int MachineId { get; set; }
        public double PowerKW { get; set; }
        public double VoltageV { get; set; }
        public double CurrentA { get; set; }
        public double PowerFactor { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
