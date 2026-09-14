using PosHC.Application.DTOs;

namespace PosHC.Application.Interfaces
{
    public interface IPatientVisitService
    {
        Task<PagedResult<PatientVisitDto>> GetPageAsync(Guid patientId, int page, int pageSize,
            CancellationToken cancellationToken, string? sortBy = null, string? sortDirection = null);
        Task<PatientVisitDto> UpdateAsync(Guid patientId, Guid visitId, VisitNotesInput input, CancellationToken cancellationToken);
    }
}
