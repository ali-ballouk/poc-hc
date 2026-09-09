using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.Interfaces;
using PosHC.Application.Services;
using PosHC.Application.DTOs;
using PosHC.Application.Validation;
using PosHC.Domain.Entities;

namespace PosHCExternal.web.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(IStaffService service, IClinicStore store, IAntiforgery antiforgery, IConfiguration config) : ControllerBase
{
    [HttpGet("session"), AllowAnonymous]
    public async Task<object> Session(CancellationToken ct)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions { HttpOnly = false, Secure = Request.IsHttps, SameSite = SameSiteMode.Strict, Path = "/" });
        return new
        {
            SetupRequired = await store.Count<StaffUser>(ct: ct) == 0,
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
        var user = await service.Login(input.Username, input.Password, ct);
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
        var expected = config["Setup:Token"] ?? "";
        BusinessRules.Check(expected.Length >= 32 && CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(expected)), SHA256.HashData(Encoding.UTF8.GetBytes(input.Token))), "Invalid setup token.");
        if (await store.Count<StaffUser>(ct: ct) != 0)
        {
            throw new BusinessException("Initial setup is already complete.", 409);
        }

        await service.Save(Guid.Empty, new(input.Username, input.DisplayName, "Administrator", true, input.Password), ct, initialSetup: true);
        return Ok();
    }
    [HttpPost("reset"), AllowAnonymous]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
    public async Task<IActionResult> Reset(ResetInput input, CancellationToken ct)
    {
        await service.Reset(input.Username, input.Token, input.Password, ct);
        return Ok();
    }
    public record LoginInput(string Username, string Password);
    public record SetupInput(string Username, string DisplayName, string Password, string Token);
    public record ResetInput(string Username, string Token, string Password);
}
