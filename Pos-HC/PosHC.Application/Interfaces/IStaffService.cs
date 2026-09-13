using PosHC.Application.DTOs;
using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces;

public interface IStaffService
{
    Task<PagedResult<StaffUser>> GetPageAsync(int page, CancellationToken cancellationToken, int pageSize = 50);
    Task<StaffUser> Save(Guid id, StaffInput input, CancellationToken ct, bool initialSetup = false);
    Task<StaffUser> Login(string username, string password, CancellationToken ct);
    Task<string> IssueReset(Guid id, CancellationToken ct);
    Task Reset(string username, string token, string password, CancellationToken ct);
}
