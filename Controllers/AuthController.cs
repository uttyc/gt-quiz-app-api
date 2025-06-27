using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace GoogleTranslateHistoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("google-login")]
        [SwaggerOperation(Summary = "Initiates Google login process")]
        [SwaggerResponse(302, "Redirects to Google login page")]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action(nameof(GoogleResponse), "Auth", null, Request.Scheme);
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            return Challenge(properties, "Google");
        }

        [Authorize]
        [HttpGet("google-response")]
        [SwaggerOperation(Summary = "Handles Google login response")]
        [SwaggerResponse(200, "Returns user info")]
        [SwaggerResponse(401, "Unauthorized")]
        public IActionResult GoogleResponse()
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized("User not authenticated.");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;
            var picture = User.FindFirst("picture")?.Value;

            if (string.IsNullOrEmpty(email))
                return Unauthorized("Email claim is missing.");

            // Optional: Log all claims for debugging
            Console.WriteLine("User Claims:");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }

            return Ok(new { Email = email, Name = name, Picture = picture });
        }

        [HttpPost("logout")]
        [SwaggerOperation(Summary = "Logs the user out")]
        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties
            {
                RedirectUri = "/"
            }, CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
