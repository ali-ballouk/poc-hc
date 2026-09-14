using PosHC.Application.DTOs;

namespace PosHC.Application.Interfaces;

public interface IPatientVisitReader
{
    Task<PagedResult<PatientVisitDto>> ReadAsync(Guid patientId, int page, int pageSize, CancellationToken ct, string? sortBy = null, string? sortDirection = null);
}
