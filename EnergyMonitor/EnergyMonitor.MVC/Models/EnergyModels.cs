namespace EnergyMonitor.MVC.Models
{
    public class FactoryModel
    {
        public int Factory_Id { get; set; }
        public string? Factory_Name { get; set; }
        public string? Location { get; set; }
        public int IsActive { get; set; }
    }

    public class MachineModel
    {
        public int Machine_Id { get; set; }
        public int Factory_Id { get; set; }
        public string? Factory_Name { get; set; }
        public string? Machine_Name { get; set; }
        public string? Machine_Type { get; set; }
        public decimal Max_Power_KW { get; set; }
        public int IsActive { get; set; }
    }

    public class EnergyReadingModel
    {
        public int Reading_Id { get; set; }
        public int Machine_Id { get; set; }
        public string? Machine_Name { get; set; }
        public string? Machine_Type { get; set; }
        public decimal Power_KW { get; set; }
        public decimal Voltage_V { get; set; }
        public decimal Current_A { get; set; }
        public decimal PowerFactor { get; set; }
        public DateTime Reading_Time { get; set; }
        public bool Is_Anomaly { get; set; }
        public decimal Anomaly_Score { get; set; }
    }

    public class AlertModel
    {
        public int Alert_Id { get; set; }
        public int Machine_Id { get; set; }
        public string? Machine_Name { get; set; }
        public string? Alert_Type { get; set; }
        public string? Alert_Message { get; set; }
        public decimal Power_Value { get; set; }
        public string? Alert_Status { get; set; }
        public DateTime Created_Date { get; set; }
    }

    public class CommonResponse
    {
        public int Code { get; set; }
        public string? Message { get; set; }
        public bool Success { get; set; }
    }

    // Requests
    public class UpsertMachineRequest
    {
        public int Machine_Id { get; set; }
        public int Factory_Id { get; set; }
        public string Machine_Name { get; set; } = string.Empty;
        public string Machine_Type { get; set; } = string.Empty;
        public decimal Max_Power_KW { get; set; }
    }
}