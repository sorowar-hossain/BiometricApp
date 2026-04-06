using BiometricApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace BiometricApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly LoginService loginService;

        public AuthController(LoginService loginService)
        {
            this.loginService = loginService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await this.loginService.ValidateCredentialsAsync(request.UserName, request.Password);
            if (user == null)
                return Unauthorized("Invalid credentials");

            // Optional: generate JWT token here

            return Ok(user);
        }
    }

    public class LoginRequest
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
