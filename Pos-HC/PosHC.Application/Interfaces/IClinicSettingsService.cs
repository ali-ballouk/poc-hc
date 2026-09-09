using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces;

public interface IClinicSettingsService
{
    Task<ClinicSettings> GetAsync(CancellationToken cancellationToken);
    Task<ClinicSettings> SaveAsync(ClinicSettings input, CancellationToken cancellationToken);
}
