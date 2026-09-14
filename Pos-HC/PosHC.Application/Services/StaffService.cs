using PosHC.Application.Exceptions;
using PosHC.Application.Mapping;
using System.Security.Cryptography;
using System.Text;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using static PosHC.Application.Validation.BusinessRules;

namespace PosHC.Application.Services
{
    public class StaffService : IStaffService
    {
        private readonly IClinicStore _store;
        private readonly IPasswordCodec _passwords;
        private readonly IAuditService _auditService;
        private readonly ICurrentStaff _staff;

        public StaffService(IClinicStore store, IPasswordCodec passwords, IAuditService auditService, ICurrentStaff staff)
        {
            _store = store;
            _passwords = passwords;
            _auditService = auditService;
            _staff = staff;
        }

        public async Task<PagedResult<StaffUserDetailsDto>> GetPageAsync(int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
        {
            var pageResult = await PagedQuery.ReadAsync<StaffUser>(_store, null, page, cancellationToken, pageSize, sortBy, sortDirection);
            return ClinicDtoMapper.MapPage(pageResult, ClinicDtoMapper.ToDto);
        }
        public static readonly string[] Roles = ["Administrator", "Receptionist", "Cashier", "Doctor"];
        private static void ValidatePassword(string password) => Check(password.Length >= 12 && password.Length <= 128, "Password must contain 12–128 characters.");
        private static string Digest(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        public Task<StaffUserDetailsDto> Save(Guid id, StaffInput input, CancellationToken ct, bool initialSetup = false) => _store.Transaction(async () =>
        {
            if (initialSetup)
            {
                Check(await _store.Count<StaffUser>(ct: ct) == 0, "Initial setup is already complete.");
            }

            var username = Required(input.Username, "Username", 100).ToLowerInvariant();
            Check(username.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '-' or '_' or '@'), "Username contains unsupported characters.");
            Check(Roles.Contains(input.Role), "Invalid staff role.");
            Check(await _store.Find<StaffUser>(x => x.Username == username && x.Id != id, ct) == null, "Username already exists.");
            var row = id == Guid.Empty ? new StaffUser() : await _store.Find<StaffUser>(x => x.Id == id, ct) ?? throw new BusinessException("User not found.", 404);
            if (row.Role == "Administrator" && row.IsActive && (!input.IsActive || input.Role != "Administrator"))
            {
                Check(await _store.Count<StaffUser>(x => x.Role == "Administrator" && x.IsActive, ct) > 1, "Keep at least one active administrator.");
            }

            Check(row.Id != _staff.Id || input.IsActive, "You cannot deactivate your own account.");
            row.Username = username;
            row.DisplayName = Required(input.DisplayName, "Display name");
            row.Role = input.Role;
            row.IsActive = input.IsActive;
            if (id == Guid.Empty || !string.IsNullOrWhiteSpace(input.Password))
            {
                ValidatePassword(input.Password ?? "");
                row.PasswordHash = _passwords.Hash(row, input.Password!);
            }
            row.SecurityStamp = Guid.NewGuid().ToString();
            if (id == Guid.Empty)
            {
                _store.Add(row);
            }

            _auditService.Record("Save", "StaffUser", row.Id);
            return ClinicDtoMapper.ToDto(row);
        }, ct);
        public async Task<StaffSessionDto> Login(string username, string password, CancellationToken ct)
        {
            var row = await _store.Find<StaffUser>(x => x.Username == username.Trim().ToLower(), ct);
            if (row == null || !row.IsActive || row.LockedUntil > DateTime.UtcNow)
            {
                throw new BusinessException("Invalid credentials or account temporarily locked.", 401);
            }

            if (!_passwords.Verify(row, password))
            {
                row.FailedAttempts++;
                if (row.FailedAttempts >= 5)
                {
                    row.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                    row.FailedAttempts = 0;
                }
                _auditService.Record("LoginFailed", "StaffUser", row.Id);
                await _store.Save(ct);
                throw new BusinessException("Invalid credentials or account temporarily locked.", 401);
            }
            row.FailedAttempts = 0;
            row.LockedUntil = null;
            _store.Add(new AuditEntry { Actor = row.Username, Action = "Login", Entity = "StaffUser", EntityId = row.Id.ToString() });
            await _store.Save(ct);
            return new StaffSessionDto(row.Id, row.Username, row.Role, row.SecurityStamp);
        }
        public async Task<string> IssueReset(Guid id, CancellationToken ct)
        {
            var row = await _store.Find<StaffUser>(x => x.Id == id, ct) ?? throw new BusinessException("User not found.", 404);
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            row.ResetTokenHash = Digest(token);
            row.ResetExpires = DateTime.UtcNow.AddMinutes(30);
            _auditService.Record("IssuePasswordReset", "StaffUser", id);
            await _store.Save(ct);
            return token;
        }
        public async Task Reset(string username, string token, string password, CancellationToken ct)
        {
            ValidatePassword(password);
            var digest = Digest(token);
            var row = await _store.Find<StaffUser>(x => x.Username == username.ToLower().Trim() && x.ResetTokenHash == digest && x.ResetExpires > DateTime.UtcNow, ct) ?? throw new BusinessException("Invalid or expired reset token.");
            row.PasswordHash = _passwords.Hash(row, password);
            row.ResetTokenHash = null;
            row.ResetExpires = null;
            row.SecurityStamp = Guid.NewGuid().ToString();
            row.LockedUntil = null;
            row.FailedAttempts = 0;
            _auditService.Record("ResetPassword", "StaffUser", row.Id);
            await _store.Save(ct);
        }

        public async Task<bool> IsSetupRequiredAsync(CancellationToken cancellationToken)
        {
            return await _store.Count<StaffUser>(ct: cancellationToken) == 0;
        }

        public async Task SetupAsync(SetupInput input, string expectedToken, CancellationToken cancellationToken)
        {
            Check(expectedToken.Length >= 32 && CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(expectedToken)),
                SHA256.HashData(Encoding.UTF8.GetBytes(input.Token))), "Invalid setup token.");
            if (!await IsSetupRequiredAsync(cancellationToken))
            {
                throw new BusinessException("Initial setup is already complete.", 409);
            }

            await Save(Guid.Empty, new StaffInput(input.Username, input.DisplayName, "Administrator", true, input.Password),
                cancellationToken, initialSetup: true);
        }

        public async Task<bool> ValidateSessionAsync(Guid id, string? securityStamp, CancellationToken cancellationToken)
        {
            var user = await _store.Find<StaffUser>(row => row.Id == id, cancellationToken);
            return user != null && user.IsActive && user.SecurityStamp == securityStamp;
        }
    }
}
