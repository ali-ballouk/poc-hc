using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces
{

    public interface IPOSHCRepository
    {
        Task<List<Doctor>> GetAllDoctorsAsync(CancellationToken cancellationToken = default);

        Task<List<Patient>> GetAllPatientsAsync(CancellationToken cancellationToken = default);
        Task<List<Item>> GetAllItemsAsync(CancellationToken cancellationToken = default);

    }
}
