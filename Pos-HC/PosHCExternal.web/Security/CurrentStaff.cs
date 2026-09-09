using System.Security.Claims;
using PosHC.Application.Interfaces;

namespace PosHCExternal.web.Security;

public class CurrentStaff(IHttpContextAccessor accessor) : ICurrentStaff
{
    public Guid Id => Guid.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
    public string Name => accessor.HttpContext?.User.Identity?.Name ?? "anonymous";
    public string Role => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role) ?? "";
}
