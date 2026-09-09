using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using static PosHC.Application.Validation.BusinessRules;

namespace PosHC.Application.Services;

public class ClinicSettingsService(IClinicStore store, IAuditService auditService) : IClinicSettingsService
{
    public async Task<ClinicSettings> GetAsync(CancellationToken cancellationToken)
    {
        return await store.Find<ClinicSettings>(settings => settings.Id == 1, cancellationToken)
            ?? throw new BusinessException("Run database migrations first.", 503);
    }

    public async Task<ClinicSettings> SaveAsync(ClinicSettings input, CancellationToken cancellationToken)
    {
        Check(input.LbpPerUsd > 0 && input.LbpPerUsd < 100_000_000, "Enter the agreed LBP per USD exchange rate.");
        Check(input.TaxRate >= 0 && input.TaxRate <= 100, "Tax rate must be between 0 and 100.");

        var settings = await GetAsync(cancellationToken);
        settings.Name = Required(input.Name, "Clinic name");
        settings.Address = input.Address ?? "";
        settings.Phone = input.Phone ?? "";
        settings.LbpPerUsd = input.LbpPerUsd;
        settings.ExchangeRateConfirmed = true;
        settings.TaxRate = input.TaxRate;
        settings.TaxRegistrationNumber = input.TaxRegistrationNumber ?? "";

        auditService.Record("Update", "ClinicSettings", settings.Id, $"LBP/USD={settings.LbpPerUsd}; tax={settings.TaxRate}");
        await store.Save(cancellationToken);
        return settings;
    }
}
