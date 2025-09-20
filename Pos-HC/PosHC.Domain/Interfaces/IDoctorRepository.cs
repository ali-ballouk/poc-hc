using PosHC.Domain.Entities;

namespace PosHC.Domain.Interfaces
{
    public interface IDoctorRepository
    {
        Task<List<Doctor>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
