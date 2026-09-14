using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.Interfaces;
using PosHC.Application.DTOs;

namespace PosHCExternal.web.Controllers
{
    [ApiController, Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IStaffService _service;
        private readonly IAntiforgery _antiforgery;
        private readonly IConfiguration _config;

        public AuthController(IStaffService service, IAntiforgery antiforgery, IConfiguration config)
        {
            _service = service;
            _antiforgery = antiforgery;
            _config = config;
        }

        [HttpGet("session"), AllowAnonymous]
        public async Task<object> Session(CancellationToken ct)
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions { HttpOnly = false, Secure = Request.IsHttps, SameSite = SameSiteMode.Strict, Path = "/" });
            return new
            {
                SetupRequired = await _service.IsSetupRequiredAsync(ct),
                User = User.Identity?.IsAuthenticated == true ? new
                {
                    Username = User.Identity.Name,
                    Role = User.FindFirstValue(ClaimTypes.Role)
                } : null
            };
        }
        [HttpPost("login"), AllowAnonymous]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
        public async Task<IActionResult> Login(LoginInput input, CancellationToken ct)
        {
            var user = await _service.Login(input.Username, input.Password, ct);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, user.Role), new Claim("stamp", user.SecurityStamp) }, CookieAuthenticationDefaults.AuthenticationScheme));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return Ok(new
            {
                user.Username,
                user.Role
            });
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return NoContent();
        }
        [HttpPost("setup"), AllowAnonymous]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
        public async Task<IActionResult> Setup(SetupInput input, CancellationToken ct)
        {
            await _service.SetupAsync(input, _config["Setup:Token"] ?? "", ct);
            return Ok();
        }
        [HttpPost("reset"), AllowAnonymous]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
        public async Task<IActionResult> Reset(ResetInput input, CancellationToken ct)
        {
            await _service.Reset(input.Username, input.Token, input.Password, ct);
            return Ok();
        }
    }
}
