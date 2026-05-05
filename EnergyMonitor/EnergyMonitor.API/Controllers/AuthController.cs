using EnergyMonitor.API.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace EnergyMonitor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IEnergyRepository _repository;

        public AuthController(IEnergyRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            var user = await _repository.GetUserByEmail(request.Email);
            if (user == null)
                return Unauthorized(
                    new { message = "Invalid email or password" });

            var hash = HashPassword(request.Password);
            if (hash != user.Password_Hash)
                return Unauthorized(
                    new { message = "Invalid email or password" });

            return Ok(new
            {
                user.User_Id,
                user.Full_Name,
                user.Email,
                user.Role
            });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(
                Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}