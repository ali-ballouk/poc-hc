using PosHC.Domain.Entities;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHC.Application.Interfaces
{
    public interface IPatientService
    {
        Task<List<PatientLookupDto>> GetAllPatientInfo(CancellationToken cancellationToken = default);

        Task<PatientDetailsDto> SaveAsync(Guid id, PatientDetailsDto input, CancellationToken ct);
        Task<PagedResult<PatientDetailsDto>> GetPageAsync(string search, int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null);

    }
}
