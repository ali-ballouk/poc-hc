using PosHC.Application.DTOs;
using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces;

public interface IStaffService
{
    Task<PagedResult<StaffUserDetailsDto>> GetPageAsync(int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null);
    Task<StaffUserDetailsDto> Save(Guid id, StaffInput input, CancellationToken ct, bool initialSetup = false);
    Task<StaffSessionDto> Login(string username, string password, CancellationToken ct);
    Task<string> IssueReset(Guid id, CancellationToken ct);
    Task Reset(string username, string token, string password, CancellationToken ct);

    Task<bool> IsSetupRequiredAsync(CancellationToken cancellationToken);
    Task SetupAsync(SetupInput input, string expectedToken, CancellationToken cancellationToken);
    Task<bool> ValidateSessionAsync(Guid id, string? securityStamp, CancellationToken cancellationToken);
}
