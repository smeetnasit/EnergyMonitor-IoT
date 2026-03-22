namespace EnergyMonitor.Simulation
{
    public class MachineSimulator
    {
        public int MachineId { get; set; }
        public string MachineName { get; set; } = string.Empty;
        public double MaxPowerKW { get; set; }
        public double BaseLoadPercent { get; set; }
        public string MachineType { get; set; } = string.Empty;

        private readonly Random _random = new Random();

        public SimulatedReading GenerateReading()
        {
            // 85% normal, 15% anomaly
            bool isSpike = _random.Next(100) < 15;

            double powerKW;
            if (isSpike)
            {
                bool overload = _random.Next(2) == 0;
                powerKW = overload
                    ? MaxPowerKW * (1.2 + _random.NextDouble() * 0.3)
                    : MaxPowerKW * (_random.NextDouble() * 0.2);
            }
            else
            {
                double variation = (_random.NextDouble() - 0.5) * 0.2;
                powerKW = MaxPowerKW * (BaseLoadPercent + variation);
                powerKW = Math.Max(0, Math.Min(powerKW, MaxPowerKW));
            }

            double voltage = 380 + _random.NextDouble() * 20;
            double powerFactor = 0.85 + _random.NextDouble() * 0.1;
            double current = (powerKW * 1000) /
                             (voltage * powerFactor * 1.732);

            return new SimulatedReading
            {
                MachineId = MachineId,
                PowerKW = Math.Round(powerKW, 3),
                VoltageV = Math.Round(voltage, 2),
                CurrentA = Math.Round(current, 3),
                PowerFactor = Math.Round(powerFactor, 3),
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public class SimulatedReading
    {
        public int MachineId { get; set; }
        public double PowerKW { get; set; }
        public double VoltageV { get; set; }
        public double CurrentA { get; set; }
        public double PowerFactor { get; set; }
        public DateTime Timestamp { get; set; }
    }
}