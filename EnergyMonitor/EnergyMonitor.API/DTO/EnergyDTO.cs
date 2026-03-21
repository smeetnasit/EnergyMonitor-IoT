namespace EnergyMonitor.API.DTO
{
    public class FactoryDTO
    {
        public int Factory_Id { get; set; }
        public string? Factory_Name { get; set; }
        public string? Location { get; set; }
        public int IsActive { get; set; }
        public DateTime? Created_Date { get; set; }
    }

    public class MachineDTO
    {
        public int Machine_Id { get; set; }
        public int Factory_Id { get; set; }
        public string? Factory_Name { get; set; }
        public string? Machine_Name { get; set; }
        public string? Machine_Type { get; set; }
        public decimal Max_Power_KW { get; set; }
        public int IsActive { get; set; }
        public DateTime? Created_Date { get; set; }
    }

    public class EnergyReadingDTO
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

    public class AlertDTO
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

    public class EnergyCostConfigDTO
    {
        public int Config_Id { get; set; }
        public int Factory_Id { get; set; }
        public decimal Cost_Per_KWH { get; set; }
        public int Peak_Hours_From { get; set; }
        public int Peak_Hours_To { get; set; }
        public decimal Peak_Multiplier { get; set; }
        public DateTime Valid_From { get; set; }
    }

    // Request DTOs (what we receive from client)
    public class UpsertMachineRequest
    {
        public int Machine_Id { get; set; }
        public int Factory_Id { get; set; }
        public string Machine_Name { get; set; } = string.Empty;
        public string Machine_Type { get; set; } = string.Empty;
        public decimal Max_Power_KW { get; set; }
    }

    public class InsertReadingRequest
    {
        public int Machine_Id { get; set; }
        public decimal Power_KW { get; set; }
        public decimal Voltage_V { get; set; }
        public decimal Current_A { get; set; }
        public decimal PowerFactor { get; set; }
    }

    public class UpdateAlertStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
