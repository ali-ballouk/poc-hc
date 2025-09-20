using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHC.Application.Interfaces
{
    public interface IDoctorService
    {
        Task<List<DoctorLookupDto>> GetAllDoctorInfo(CancellationToken cancellationToken = default);

    }
}