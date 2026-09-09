using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces
{
    public interface IDoctorService
    {
        Task<List<DoctorLookupDto>> GetAllDoctorInfo(CancellationToken cancellationToken = default);
        DoctorLookupDto GetDoctor(Guid doctorId);
        Task<DoctorAvailability> SaveAvailabilityAsync(DoctorAvailability input, CancellationToken cancellationToken);
        Task<PagedResult<DoctorAvailability>> GetAvailabilityAsync(int page, CancellationToken cancellationToken);

        Task<Doctor> SaveAsync(Guid id, Doctor input, CancellationToken ct);
        Task<PagedResult<Doctor>> GetPageAsync(string search, int page, CancellationToken cancellationToken);

    }
}
