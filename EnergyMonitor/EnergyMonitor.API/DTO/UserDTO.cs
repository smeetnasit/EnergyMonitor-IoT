namespace EnergyMonitor.API.DTO
{
    public class UserDTO
    {
        public int User_Id { get; set; }
        public string? Full_Name { get; set; }
        public string? Email { get; set; }
        public string? Password_Hash { get; set; }
        public string? Role { get; set; }
        public int IsActive { get; set; }
    }
}
