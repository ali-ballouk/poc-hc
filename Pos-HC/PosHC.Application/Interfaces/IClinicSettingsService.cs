using PosHC.Application.DTOs;
using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces;

public interface IClinicSettingsService
{
    Task<ClinicSettingsDetailsDto> GetAsync(CancellationToken cancellationToken);
    Task<ClinicSettingsDetailsDto> SaveAsync(ClinicSettingsDetailsDto input, CancellationToken cancellationToken);
}
