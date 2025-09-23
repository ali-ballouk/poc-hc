using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces
{
    public interface IDoctorService
    {
        Task<List<DoctorLookupDto>> GetAllDoctorInfo(CancellationToken cancellationToken = default);
        DoctorLookupDto GetDoctor(Guid doctorId);

    }
}