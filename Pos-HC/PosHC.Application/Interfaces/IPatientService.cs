using PosHC.Domain.Entities;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHC.Application.Interfaces
{
    public interface IPatientService
    {
        Task<List<PatientLookupDto>> GetAllPatientInfo(CancellationToken cancellationToken = default);
        PatientLookupDto GetPatient(Guid patientId);

        Task<Patient> SaveAsync(Guid id, Patient input, CancellationToken ct);
        Task<PagedResult<Patient>> GetPageAsync(string search, int page, CancellationToken cancellationToken, int pageSize = 50);

    }
}
