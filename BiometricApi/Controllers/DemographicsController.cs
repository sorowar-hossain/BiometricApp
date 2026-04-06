using BiometricApi.Entities;
using BiometricApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BiometricApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemographicsController : ControllerBase
    {
        private readonly DemographicsService service;

        public DemographicsController(DemographicsService service)
        {
            this.service = service;
        }

        [HttpPost("{orgId}")]
        public async Task<IActionResult> Create(Demographic demographic)
        {
            await service.CreateAsync(demographic);
            return Ok();
        }
    }
}
