using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHC.Application.Services;

public class AuditService(IClinicStore store, ICurrentStaff staff) : IAuditService
{
    public void Record(string action, string entity, object id, string details = "")
    {
        store.Add(new AuditEntry
        {
            Actor = staff.Name,
            Action = action,
            Entity = entity,
            EntityId = id.ToString()!,
            Details = details
        });
    }

    public async Task RecordAndSaveAsync(string action, string entity, object id, CancellationToken cancellationToken)
    {
        Record(action, entity, id);
        await store.Save(cancellationToken);
    }

    public Task<PagedResult<AuditEntry>> GetPageAsync(string search, int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
    {
        return PagedQuery.ReadAsync<AuditEntry>(store,
            entry => entry.Actor.Contains(search) || entry.Entity.Contains(search) || entry.Action.Contains(search),
            page, cancellationToken, pageSize, sortBy, sortDirection);
    }
}
