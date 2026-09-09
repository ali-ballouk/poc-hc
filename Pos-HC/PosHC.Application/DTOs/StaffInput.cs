namespace PosHC.Application.DTOs;

public record StaffInput(string Username, string DisplayName, string Role, bool IsActive, string? Password);
