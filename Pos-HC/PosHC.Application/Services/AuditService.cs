using PosHC.Application.Mapping;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHC.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IClinicStore _store;
        private readonly ICurrentStaff _staff;

        public AuditService(IClinicStore store, ICurrentStaff staff)
        {
            _store = store;
            _staff = staff;
        }

        public void Record(string action, string entity, object id, string details = "")
        {
            _store.Add(new AuditEntry
            {
                Actor = _staff.Name,
                Action = action,
                Entity = entity,
                EntityId = id.ToString()!,
                Details = details
            });
        }

        public async Task RecordAndSaveAsync(string action, string entity, object id, CancellationToken cancellationToken)
        {
            Record(action, entity, id);
            await _store.Save(cancellationToken);
        }

        public async Task<PagedResult<AuditEntryDetailsDto>> GetPageAsync(string search, int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
        {
            var pageResult = await PagedQuery.ReadAsync<AuditEntry>(_store,
                entry => entry.Actor.Contains(search) || entry.Entity.Contains(search) || entry.Action.Contains(search),
                page, cancellationToken, pageSize, sortBy, sortDirection);
            return ClinicDtoMapper.MapPage(pageResult, ClinicDtoMapper.ToDto);
        }
    }
}
