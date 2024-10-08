using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Glosso.Models;
using Glosso.Services;
using System.Threading.Tasks;

namespace Glosso.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IAntiforgery _antiforgery;

        public AuthController(UserService userService, IAntiforgery antiforgery)
        {
            _userService = userService;
            _antiforgery = antiforgery;
        }

        // Endpoint to get antiforgery tokens
        [HttpGet("tokens")]
        public IActionResult GetTokens()
        {
            var tokens = _antiforgery.GetTokens(HttpContext);
            return Ok(new { RequestToken = tokens.RequestToken });
        }

        // Login endpoint
        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Here we pass the antiforgery token to the UserService Login method
            var result = await _userService.Login(model.Username, model.Password, model.AntiforgeryToken);
            if (!result)
            {
                return Unauthorized();
            }

            return Ok(); // Return success response
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string AntiforgeryToken { get; set; } // Make sure this property exists
    }

}