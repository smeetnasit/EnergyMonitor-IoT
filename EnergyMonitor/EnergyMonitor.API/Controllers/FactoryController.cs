using EnergyMonitor.API.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMonitor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FactoryController : ControllerBase
    {
        private readonly IEnergyRepository _repository;

        public FactoryController(IEnergyRepository repository)
        {
            _repository = repository;
        }

        // GET: api/factory
        [HttpGet]
        public async Task<IActionResult> GetAllFactories()
        {
            var factories = await _repository.GetAllFactories();
            return Ok(factories);
        }
    }
}