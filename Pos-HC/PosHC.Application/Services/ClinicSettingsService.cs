using PosHC.Application.Exceptions;
using PosHC.Application.Mapping;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using static PosHC.Application.Validation.BusinessRules;

namespace PosHC.Application.Services
{
    public class ClinicSettingsService : IClinicSettingsService
    {
        private readonly IClinicStore _store;
        private readonly IAuditService _auditService;

        public ClinicSettingsService(IClinicStore store, IAuditService auditService)
        {
            _store = store;
            _auditService = auditService;
        }

        public async Task<ClinicSettingsDetailsDto> GetAsync(CancellationToken cancellationToken)
        {
            var settings = await _store.Find<ClinicSettings>(settings => settings.Id == 1, cancellationToken)
                ?? throw new BusinessException("Run database migrations first.", 503);
            return ClinicDtoMapper.ToDto(settings);
        }

        public async Task<ClinicSettingsDetailsDto> SaveAsync(ClinicSettingsDetailsDto input, CancellationToken cancellationToken)
        {
            Check(input.LbpPerUsd > 0 && input.LbpPerUsd < 100_000_000, "Enter the agreed LBP per USD exchange rate.");
            Check(input.TaxRate >= 0 && input.TaxRate <= 100, "Tax rate must be between 0 and 100.");
            Check((input.Address?.Length ?? 0) <= 500, "Clinic address must be at most 500 characters.");
            Check((input.Phone?.Length ?? 0) <= 50, "Clinic phone must be at most 50 characters.");
            if (input.SingleDoctorMode)
            {
                Check(input.DefaultDoctorId.HasValue && await _store.Find<Doctor>(doctor => doctor.Id == input.DefaultDoctorId && doctor.IsActive, cancellationToken) != null,
                    "Choose an active doctor for single-doctor mode.");
            }

            var settings = await _store.Find<ClinicSettings>(row => row.Id == 1, cancellationToken)
                ?? throw new BusinessException("Run database migrations first.", 503);
            settings.Name = Required(input.Name, "Clinic name");
            settings.Address = input.Address ?? "";
            settings.Phone = input.Phone ?? "";
            settings.LbpPerUsd = input.LbpPerUsd;
            settings.ExchangeRateConfirmed = true;
            settings.TaxRate = input.TaxRate;
            settings.TaxRegistrationNumber = input.TaxRegistrationNumber ?? "";
            settings.SingleDoctorMode = input.SingleDoctorMode;
            settings.DefaultDoctorId = input.SingleDoctorMode ? input.DefaultDoctorId : null;

            _auditService.Record("Update", "ClinicSettings", settings.Id, $"LBP/USD={settings.LbpPerUsd}; tax={settings.TaxRate}; singleDoctor={settings.SingleDoctorMode}; doctor={settings.DefaultDoctorId}");
            await _store.Save(cancellationToken);
            return ClinicDtoMapper.ToDto(settings);
        }
    }
}
