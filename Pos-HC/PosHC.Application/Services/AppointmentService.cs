using PosHC.Application.Exceptions;
using PosHC.Application.Mapping;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using static PosHC.Application.Validation.BusinessRules;

namespace PosHC.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IClinicStore _store;
        private readonly IAuditService _auditService;

        public AppointmentService(IClinicStore store, IAuditService auditService)
        {
            _store = store;
            _auditService = auditService;
        }

        private static readonly string[] AllowedStatuses = ["Booked", "CheckedIn", "InProgress", "Completed", "Cancelled", "NoShow"];

        public async Task<PagedResult<AppointmentDetailsDto>> GetPageAsync(DateTime? from, DateTime? to, int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
        {
            var pageResult = await PagedQuery.ReadAsync<Appointment>(_store,
                appointment => (!from.HasValue || appointment.StartsAt >= from) && (!to.HasValue || appointment.StartsAt < to),
                page, cancellationToken, pageSize, sortBy, sortDirection);
            return ClinicDtoMapper.MapPage(pageResult, ClinicDtoMapper.ToDto);
        }

        public Task<AppointmentDetailsDto> SaveAsync(Guid id, AppointmentDetailsDto input, CancellationToken cancellationToken)
        {
            return _store.Transaction(async () =>
            {
                if (id == Guid.Empty)
                {
                    input.DoctorId = await ClinicDoctor.ResolveAsync(_store, input.DoctorId, cancellationToken);
                }

                Check(input.StartsAt.Kind == DateTimeKind.Utc && input.EndsAt.Kind == DateTimeKind.Utc,
                    "Appointment times must include a UTC timezone.");
                Check(input.EndsAt > input.StartsAt && input.EndsAt - input.StartsAt <= TimeSpan.FromHours(8),
                    "Appointment duration must be between 1 minute and 8 hours.");
                Check(await _store.Find<Patient>(patient => patient.Id == input.PatientId && patient.IsActive, cancellationToken) != null,
                    "Select an active patient.");
                Check(await _store.Find<Doctor>(doctor => doctor.Id == input.DoctorId && doctor.IsActive, cancellationToken) != null,
                    "Select an active doctor.");

                var appointment = id == Guid.Empty
                    ? new Appointment()
                    : await _store.Find<Appointment>(existing => existing.Id == id, cancellationToken)
                        ?? throw new BusinessException("Appointment not found.", 404);

                Check(AllowedStatuses.Contains(input.Status), "Invalid appointment status.");
                if (input.Status is not ("Cancelled" or "NoShow"))
                {
                    var overlappingAppointment = await _store.Find<Appointment>(existing =>
                        existing.Id != id && existing.DoctorId == input.DoctorId &&
                        existing.Status != "Cancelled" && existing.Status != "NoShow" &&
                        existing.StartsAt < input.EndsAt && existing.EndsAt > input.StartsAt, cancellationToken);
                    Check(overlappingAppointment == null, "The doctor already has an appointment in this time slot.");

                    var availability = await _store.Find<DoctorAvailability>(slot =>
                        slot.DoctorId == input.DoctorId && slot.StartsAt <= input.StartsAt && slot.EndsAt >= input.EndsAt,
                        cancellationToken);
                    Check(availability != null, "Add doctor availability covering this appointment first.");
                }

                appointment.PatientId = input.PatientId;
                appointment.DoctorId = input.DoctorId;
                appointment.StartsAt = input.StartsAt;
                appointment.EndsAt = input.EndsAt;
                appointment.Reason = input.Reason?.Trim() ?? "";
                Check(appointment.Reason.Length <= 500, "Reason must be at most 500 characters.");
                appointment.Status = input.Status;

                if (id == Guid.Empty)
                {
                    _store.Add(appointment);
                }

                _auditService.Record("Save", "Appointment", appointment.Id, appointment.Status);
                return ClinicDtoMapper.ToDto(appointment);
            }, cancellationToken);
        }
    }
}
