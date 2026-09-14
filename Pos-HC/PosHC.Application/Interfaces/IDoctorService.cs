using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces
{
    public interface IDoctorService
    {
        Task<List<DoctorLookupDto>> GetAllDoctorInfo(CancellationToken cancellationToken = default);
        Task<DoctorAvailabilityDetailsDto> SaveAvailabilityAsync(DoctorAvailabilityDetailsDto input, CancellationToken cancellationToken);
        Task<PagedResult<DoctorAvailabilityDetailsDto>> GetAvailabilityAsync(int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null);

        Task<DoctorDetailsDto> SaveAsync(Guid id, DoctorDetailsDto input, CancellationToken ct);
        Task<PagedResult<DoctorDetailsDto>> GetPageAsync(string search, int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null);

    }
}
