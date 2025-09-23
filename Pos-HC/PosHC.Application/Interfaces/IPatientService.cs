using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHC.Application.Interfaces
{
    public interface IPatientService
    {
        Task<List<PatientLookupDto>> GetAllPatientInfo(CancellationToken cancellationToken = default);
        PatientLookupDto GetPatient(Guid patientId);

    }
}