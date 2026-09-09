using PosHC.Application.DTOs;
using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces;

public interface IAppointmentService
{
    Task<Appointment> SaveAsync(Guid id, Appointment input, CancellationToken cancellationToken);
    Task<PagedResult<Appointment>> GetPageAsync(DateTime? from, DateTime? to, int page, CancellationToken cancellationToken);
}
