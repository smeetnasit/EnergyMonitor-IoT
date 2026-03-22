using Microsoft.ML;
using Microsoft.ML.Data;

namespace EnergyMonitor.API.Services
{
    public class AnomalyDetectionService
    {
        private readonly ILogger<AnomalyDetectionService> _logger;
        private readonly MLContext _mlContext;

        // Store recent readings per machine for ML analysis
        // Key = MachineId, Value = last 30 readings
        private readonly Dictionary<int, List<float>> _machineReadings
            = new();

        private const int MinReadingsForDetection = 10;
        private const int MaxReadingsToStore = 50;
        private const double AnomalyThreshold = 0.90;

        public AnomalyDetectionService(
            ILogger<AnomalyDetectionService> logger)
        {
            _logger = logger;
            _mlContext = new MLContext(seed: 0);
        }

        public AnomalyResult DetectAnomaly(
            int machineId, float powerKW)
        {
            // Initialize history for new machine
            if (!_machineReadings.ContainsKey(machineId))
                _machineReadings[machineId] = new List<float>();

            var history = _machineReadings[machineId];

            // Add current reading to history
            history.Add(powerKW);

            // Keep only last MaxReadingsToStore readings
            if (history.Count > MaxReadingsToStore)
                history.RemoveAt(0);

            // Need minimum readings before we can detect
            if (history.Count < MinReadingsForDetection)
            {
                return new AnomalyResult
                {
                    IsAnomaly = false,
                    AnomalyScore = 0,
                    Message = "Collecting baseline data..."
                };
            }

            try
            {
                // Create data for ML.NET
                var data = history.Select(v =>
                    new PowerReading { PowerKW = v }).ToList();

                var dataView = _mlContext.Data
                    .LoadFromEnumerable(data);

                // ✅ IID Spike Detection
                // Detects sudden spikes in time-series data
                var pipeline = _mlContext.Transforms
                    .DetectIidSpike(
                        outputColumnName: "Prediction",
                        inputColumnName: nameof(PowerReading.PowerKW),
                        confidence: 95,        // 95% confidence
                        pvalueHistoryLength:
                            history.Count / 2  // half the history
                    );

                var model = pipeline.Fit(dataView);
                var transformed = model.Transform(dataView);

                // Get prediction for the LAST reading (current one)
                var predictions = _mlContext.Data
                    .CreateEnumerable<PowerPrediction>(
                        transformed, reuseRowObject: false)
                    .ToList();

                var lastPrediction = predictions.LastOrDefault();

                if (lastPrediction?.Prediction == null)
                    return new AnomalyResult { IsAnomaly = false };

                // Prediction[0] = alert (0 or 1)
                // Prediction[1] = raw score
                // Prediction[2] = p-value (lower = more anomalous)
                bool isSpike = lastPrediction.Prediction[0] == 1;
                double pValue = lastPrediction.Prediction[2];
                double anomalyScore = 1 - pValue; // higher = more anomalous

                if (isSpike)
                {
                    _logger.LogWarning(
                        "⚠️ ANOMALY detected! Machine {MachineId} | " +
                        "Power: {Power}KW | Score: {Score:F3}",
                        machineId, powerKW, anomalyScore);
                }

                return new AnomalyResult
                {
                    IsAnomaly = isSpike,
                    AnomalyScore = (decimal)anomalyScore,
                    Message = isSpike
                        ? $"Spike detected! Power: {powerKW}KW"
                        : "Normal"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "ML detection error: {Error}", ex.Message);
                return new AnomalyResult { IsAnomaly = false };
            }
        }

        // Add to history without detection (for initialization)
        public void AddToHistory(int machineId, float powerKW)
        {
            if (!_machineReadings.ContainsKey(machineId))
                _machineReadings[machineId] = new List<float>();
            _machineReadings[machineId].Add(powerKW);
        }
    }

    // ML.NET input class
    public class PowerReading
    {
        [LoadColumn(0)]
        public float PowerKW { get; set; }
    }

    // ML.NET output class
    public class PowerPrediction
    {
        [VectorType(3)]
        public double[]? Prediction { get; set; }
    }

    // Result returned to caller
    public class AnomalyResult
    {
        public bool IsAnomaly { get; set; }
        public decimal AnomalyScore { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}