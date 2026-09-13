using static PosHC.Application.Validation.BusinessRules;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHC.Application.Services
{
    public class DoctorService : IDoctorService
    {

        private readonly IPOSHCRepository _poshsRepository;
        private readonly IClinicStore _store;
        private readonly IAuditService _auditService;

        public DoctorService(IPOSHCRepository doctorRepository, IClinicStore store, IAuditService auditService)
        {
            _poshsRepository = doctorRepository;
            _store = store;
            _auditService = auditService;
        }

        public async Task<List<DoctorLookupDto>> GetAllDoctorInfo(CancellationToken cancellationToken = default)
        {
            var doctors = await GetAllDoctorAsync(cancellationToken);

            return doctors.Where(x => x.IsActive).Select(DoctorLookupDtoMapper).ToList();
        }


        public DoctorLookupDto GetDoctor(Guid doctorId)
        {
            var doctor = GetDoctorAsync(doctorId).GetAwaiter().GetResult();
            return DoctorLookupDtoMapper(doctor);
        }

        private async Task<Doctor> GetDoctorAsync(Guid doctorId, CancellationToken cancellationToken = default)
        {
            var doctors = await GetAllDoctorAsync(cancellationToken);
            var doctor = doctors.Find(d => d.Id == doctorId);
            if (doctor == null)
            {
                throw new Exception("Doctor not found.");
            }

            return doctor;
        }
        private async Task<List<Doctor>> GetAllDoctorAsync(CancellationToken cancellationToken = default)
        {
            var doctors = await _poshsRepository.GetAllDoctorsAsync(cancellationToken);
            return doctors;
        }


        public Task<PagedResult<DoctorAvailability>> GetAvailabilityAsync(int page, CancellationToken cancellationToken, int pageSize = 50)
        {
            return PagedQuery.ReadAsync<DoctorAvailability>(_store, null, page, cancellationToken, pageSize);
        }

        public async Task<DoctorAvailability> SaveAvailabilityAsync(DoctorAvailability input, CancellationToken cancellationToken)
        {
            input.DoctorId = await ClinicDoctor.ResolveAsync(_store, input.DoctorId, cancellationToken);
            Check(input.StartsAt.Kind == DateTimeKind.Utc && input.EndsAt.Kind == DateTimeKind.Utc && input.EndsAt > input.StartsAt,
                "Provide a valid availability period with timezone.");
            Check(await _store.Find<Doctor>(doctor => doctor.Id == input.DoctorId && doctor.IsActive, cancellationToken) != null,
                "Select an active doctor.");

            var availability = new DoctorAvailability
            {
                DoctorId = input.DoctorId,
                StartsAt = input.StartsAt,
                EndsAt = input.EndsAt
            };
            _store.Add(availability);
            _auditService.Record("Create", "DoctorAvailability", availability.Id);
            await _store.Save(cancellationToken);
            return availability;
        }

        DoctorLookupDto DoctorLookupDtoMapper(Doctor doctor)
        {
            return new DoctorLookupDto
            {
                Id = doctor.Id,
                FullName = string.Format("{0} {1}", doctor.FirstName, doctor.LastName),
                Fee = doctor.Fee
            };
        }
        public async Task<Doctor> SaveAsync(Guid id, Doctor input, CancellationToken ct)
        {
            if (!input.IsActive)
            {
                var settings = await _store.Find<ClinicSettings>(s => s.Id == 1, ct);
                Check(settings?.SingleDoctorMode != true || settings.DefaultDoctorId != id,
                    "Change the single-doctor setting before deactivating this doctor.");
            }
            var doctor = id == Guid.Empty ? new Doctor { Id = Guid.NewGuid() } : await _store.Find<Doctor>(x => x.Id == id, ct) ?? throw new BusinessException("Doctor not found.", 404);
            doctor.FirstName = Required(input.FirstName, "First name");
            doctor.LastName = Required(input.LastName, "Last name");
            Check(input.Fee >= 0 && input.Fee <= 1_000_000 && decimal.Round(input.Fee, 2) == input.Fee, "Doctor fee must be a valid USD amount.");
            doctor.Fee = input.Fee;
            doctor.Phone = input.Phone ?? "";
            doctor.Specialty = input.Specialty ?? "";
            doctor.IsActive = input.IsActive;
            if (id == Guid.Empty)
            {
                _store.Add(doctor);
            }

            _auditService.Record(id == Guid.Empty ? "Create" : "Update", "Doctor", doctor.Id);
            await _store.Save(ct);
            return doctor;
        }

        public Task<PagedResult<Doctor>> GetPageAsync(string search, int page, CancellationToken cancellationToken, int pageSize = 50) => PagedQuery.ReadAsync<Doctor>(_store, item => item.FirstName.Contains(search) || item.LastName.Contains(search), page, cancellationToken, pageSize);

    }
}
