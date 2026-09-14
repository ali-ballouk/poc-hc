using System.Security.Cryptography;
using System.Text;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using static PosHC.Application.Validation.BusinessRules;

namespace PosHC.Application.Services;

public class StaffService(IClinicStore store, IPasswordCodec passwords, IAuditService auditService, ICurrentStaff staff) : IStaffService
{
    public Task<PagedResult<StaffUser>> GetPageAsync(int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
    {
        return PagedQuery.ReadAsync<StaffUser>(store, null, page, cancellationToken, pageSize, sortBy, sortDirection);
    }
    public static readonly string[] Roles = ["Administrator", "Receptionist", "Cashier", "Doctor"];
    private static void ValidatePassword(string password) => Check(password.Length >= 12 && password.Length <= 128, "Password must contain 12–128 characters.");
    private static string Digest(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    public Task<StaffUser> Save(Guid id, StaffInput input, CancellationToken ct, bool initialSetup = false) => store.Transaction(async () =>
    {
        if (initialSetup)
        {
            Check(await store.Count<StaffUser>(ct: ct) == 0, "Initial setup is already complete.");
        }

        var username = Required(input.Username, "Username", 100).ToLowerInvariant();
        Check(username.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '-' or '_' or '@'), "Username contains unsupported characters.");
        Check(Roles.Contains(input.Role), "Invalid staff role.");
        Check(await store.Find<StaffUser>(x => x.Username == username && x.Id != id, ct) == null, "Username already exists.");
        var row = id == Guid.Empty ? new StaffUser() : await store.Find<StaffUser>(x => x.Id == id, ct) ?? throw new BusinessException("User not found.", 404);
        if (row.Role == "Administrator" && row.IsActive && (!input.IsActive || input.Role != "Administrator"))
        {
            Check(await store.Count<StaffUser>(x => x.Role == "Administrator" && x.IsActive, ct) > 1, "Keep at least one active administrator.");
        }

        Check(row.Id != staff.Id || input.IsActive, "You cannot deactivate your own account.");
        row.Username = username;
        row.DisplayName = Required(input.DisplayName, "Display name");
        row.Role = input.Role;
        row.IsActive = input.IsActive;
        if (id == Guid.Empty || !string.IsNullOrWhiteSpace(input.Password))
        {
            ValidatePassword(input.Password ?? "");
            row.PasswordHash = passwords.Hash(row, input.Password!);
        }
        row.SecurityStamp = Guid.NewGuid().ToString();
        if (id == Guid.Empty)
        {
            store.Add(row);
        }

        auditService.Record("Save", "StaffUser", row.Id);
        return row;
    }, ct);
    public async Task<StaffUser> Login(string username, string password, CancellationToken ct)
    {
        var row = await store.Find<StaffUser>(x => x.Username == username.Trim().ToLower(), ct);
        if (row == null || !row.IsActive || row.LockedUntil > DateTime.UtcNow)
        {
            throw new BusinessException("Invalid credentials or account temporarily locked.", 401);
        }

        if (!passwords.Verify(row, password))
        {
            row.FailedAttempts++;
            if (row.FailedAttempts >= 5)
            {
                row.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                row.FailedAttempts = 0;
            }
            auditService.Record("LoginFailed", "StaffUser", row.Id);
            await store.Save(ct);
            throw new BusinessException("Invalid credentials or account temporarily locked.", 401);
        }
        row.FailedAttempts = 0;
        row.LockedUntil = null;
        store.Add(new AuditEntry { Actor = row.Username, Action = "Login", Entity = "StaffUser", EntityId = row.Id.ToString() });
        await store.Save(ct);
        return row;
    }
    public async Task<string> IssueReset(Guid id, CancellationToken ct)
    {
        var row = await store.Find<StaffUser>(x => x.Id == id, ct) ?? throw new BusinessException("User not found.", 404);
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        row.ResetTokenHash = Digest(token);
        row.ResetExpires = DateTime.UtcNow.AddMinutes(30);
        auditService.Record("IssuePasswordReset", "StaffUser", id);
        await store.Save(ct);
        return token;
    }
    public async Task Reset(string username, string token, string password, CancellationToken ct)
    {
        ValidatePassword(password);
        var digest = Digest(token);
        var row = await store.Find<StaffUser>(x => x.Username == username.ToLower().Trim() && x.ResetTokenHash == digest && x.ResetExpires > DateTime.UtcNow, ct) ?? throw new BusinessException("Invalid or expired reset token.");
        row.PasswordHash = passwords.Hash(row, password);
        row.ResetTokenHash = null;
        row.ResetExpires = null;
        row.SecurityStamp = Guid.NewGuid().ToString();
        row.LockedUntil = null;
        row.FailedAttempts = 0;
        auditService.Record("ResetPassword", "StaffUser", row.Id);
        await store.Save(ct);
    }
}
