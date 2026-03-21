using EnergyMonitor.API.DTO;
using EnergyMonitor.API.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMonitor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertController : ControllerBase
    {
        private readonly IEnergyRepository _repository;

        public AlertController(IEnergyRepository repository)
        {
            _repository = repository;
        }

        // GET: api/alert?factoryId=1
        [HttpGet]
        public async Task<IActionResult> GetAlerts(
            [FromQuery] int factoryId = 1)
        {
            var alerts = await _repository.GetAlerts(factoryId);
            return Ok(alerts);
        }

        // PUT: api/alert/1/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateAlertStatus(
            int id,
            [FromBody] UpdateAlertStatusRequest request)
        {
            var validStatuses = new[] { "New", "Acknowledged", "Resolved" };
            if (!validStatuses.Contains(request.Status))
                return BadRequest(new { message = "Invalid status" });

            await _repository.UpdateAlertStatus(id, request.Status);
            return Ok(new { message = $"Alert {request.Status}!" });
        }
    }
}