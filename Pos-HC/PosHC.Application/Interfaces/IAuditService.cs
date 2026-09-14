using PosHC.Application.DTOs;
using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces;

public interface IAuditService
{
    void Record(string action, string entity, object id, string details = "");
    Task RecordAndSaveAsync(string action, string entity, object id, CancellationToken cancellationToken);
    Task<PagedResult<AuditEntry>> GetPageAsync(string search, int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null);
}
