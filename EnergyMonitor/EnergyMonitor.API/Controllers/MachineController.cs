using EnergyMonitor.API.DTO;
using EnergyMonitor.API.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMonitor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachineController : ControllerBase
    {
        private readonly IEnergyRepository _repository;

        public MachineController(IEnergyRepository repository)
        {
            _repository = repository;
        }

        // GET: api/machine?factoryId=1
        [HttpGet]
        public async Task<IActionResult> GetAllMachines(
            [FromQuery] int factoryId = 1)
        {
            var machines = await _repository.GetAllMachines(factoryId);
            return Ok(machines);
        }

        // POST: api/machine
        [HttpPost]
        public async Task<IActionResult> UpsertMachine(
            [FromBody] UpsertMachineRequest request)
        {
            if (string.IsNullOrEmpty(request.Machine_Name))
                return BadRequest(new { message = "Machine name is required" });

            await _repository.UpsertMachine(request);
            return Ok(new
            {
                message = request.Machine_Id == 0
                ? "Machine added!" : "Machine updated!"
            });
        }

        // DELETE: api/machine/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMachine(int id)
        {
            await _repository.DeleteMachine(id);
            return Ok(new { message = "Machine deactivated!" });
        }
    }
}