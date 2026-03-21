using EnergyMonitor.API.DTO;
using EnergyMonitor.API.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMonitor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnergyController : ControllerBase
    {
        private readonly IEnergyRepository _repository;

        public EnergyController(IEnergyRepository repository)
        {
            _repository = repository;
        }

        // GET: api/energy/latest?factoryId=1
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestReadings(
            [FromQuery] int factoryId = 1)
        {
            var readings = await _repository.GetLatestReadings(factoryId);
            return Ok(readings);
        }

        // GET: api/energy/history?machineId=1&hours=24
        [HttpGet("history")]
        public async Task<IActionResult> GetReadingHistory(
            [FromQuery] int machineId,
            [FromQuery] int hours = 24)
        {
            var history = await _repository
                .GetReadingHistory(machineId, hours);
            return Ok(history);
        }

        // POST: api/energy/reading
        [HttpPost("reading")]
        public async Task<IActionResult> InsertReading(
            [FromBody] InsertReadingRequest request)
        {
            await _repository.InsertReading(request, false, 0);
            return Ok(new { message = "Reading saved!" });
        }
    }
}