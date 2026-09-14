using PosHC.Application.DTOs;
using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentDetailsDto> SaveAsync(Guid id, AppointmentDetailsDto input, CancellationToken cancellationToken);
    Task<PagedResult<AppointmentDetailsDto>> GetPageAsync(DateTime? from, DateTime? to, int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null);
}
