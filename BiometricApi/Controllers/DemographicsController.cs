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

        
        public async Task<IActionResult> Create(Demographic demographic)
        {
            await service.CreateAsync(demographic);
            return Ok();
        }

        [HttpGet("personuniqueid/{orgId}")]
        public async Task<ActionResult<string>> GetPersonUniqueId(int orgId)
        {
            try
            {
                string uniqueId = await service.GetPersonUniqueId(orgId);

                if (string.IsNullOrEmpty(uniqueId))
                    return NotFound("Unique ID not found.");

                return Ok(uniqueId);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
