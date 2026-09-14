namespace PosHC.Application.DTOs
{
    public record StaffSessionDto(Guid Id, string Username, string Role, string SecurityStamp);
    public record LoginInput(string Username, string Password);
    public record SetupInput(string Username, string DisplayName, string Password, string Token);
    public record ResetInput(string Username, string Token, string Password);
    public record PasswordResetDto(string Token, int ExpiresInMinutes = 30);
}
