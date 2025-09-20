using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces
{

    public interface IDoctorRepository
    {
        Task<List<Doctor>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
