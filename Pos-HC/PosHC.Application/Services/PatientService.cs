using static PosHC.Application.Validation.BusinessRules;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
namespace PosHC.Application.Services
{
    public class PatientService : IPatientService
    {

        private readonly IPOSHCRepository _poshsRepository;
        private readonly IClinicStore _store;
        private readonly IAuditService _auditService;

        public PatientService(IPOSHCRepository patientRepository, IClinicStore store, IAuditService auditService)
        {
            _poshsRepository = patientRepository;
            _store = store;
            _auditService = auditService;
        }

        public async Task<List<PatientLookupDto>> GetAllPatientInfo(CancellationToken cancellationToken = default)
        {
            var patients = await GetAllPatients(cancellationToken);
            return patients.Where(x => x.IsActive).Select(PatientLookupDtoMapper).ToList();
        }


        public PatientLookupDto GetPatient(Guid patientId)
        {
            var patient = GetPatientAsync(patientId).GetAwaiter().GetResult();
            return PatientLookupDtoMapper(patient);
        }

        private async Task<Patient> GetPatientAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            var patients = await GetAllPatients(cancellationToken);
            var patient = patients.Find(p => p.Id == patientId);
            if (patient == null)
            {
                throw new BusinessException("Patient not found.", 404);
            }

            return patient;
        }

        public async Task<List<Patient>> GetAllPatients(CancellationToken cancellationToken = default)
        {
            var patients = await _poshsRepository.GetAllPatientsAsync(cancellationToken);
            return patients;
        }
        PatientLookupDto PatientLookupDtoMapper(Patient patient)
        {
            return new PatientLookupDto
            {
                Id = patient.Id,
                FullName = string.Format("{0} {1}", patient.FirstName, patient.LastName)
            };
        }

        public async Task<Patient> SaveAsync(Guid id, Patient input, CancellationToken ct)
        {
            var patient = id == Guid.Empty ? new Patient { Id = Guid.NewGuid() } : await _store.Find<Patient>(x => x.Id == id, ct) ?? throw new BusinessException("Patient not found.", 404);
            patient.FirstName = Required(input.FirstName, "First name");
            patient.LastName = Required(input.LastName, "Last name");
            patient.Phone = input.Phone?.Trim() ?? "";
            patient.Email = input.Email?.Trim() ?? "";
            patient.Address = input.Address?.Trim() ?? "";
            Check(patient.Phone.Length <= 50 && patient.Email.Length <= 200 && patient.Address.Length <= 500, "Contact details are too long.");
            Check(input.DateOfBirth == null || input.DateOfBirth <= DateTime.Today, "Date of birth cannot be in the future.");
            patient.DateOfBirth = input.DateOfBirth;
            patient.IsActive = input.IsActive;
            if (id == Guid.Empty)
            {
                Check(await _store.Find<Patient>(x => x.FirstName == patient.FirstName && x.LastName == patient.LastName && x.Phone == patient.Phone && x.DateOfBirth == patient.DateOfBirth, ct) == null, "A patient with these details already exists. Search before adding another.");
                _store.Add(patient);
            }
            _auditService.Record(id == Guid.Empty ? "Create" : "Update", "Patient", patient.Id);
            await _store.Save(ct);
            return patient;
        }

        public Task<PagedResult<Patient>> GetPageAsync(string search, int page, CancellationToken cancellationToken) => PagedQuery.ReadAsync<Patient>(_store, item => item.FirstName.Contains(search) || item.LastName.Contains(search) || item.Phone.Contains(search), page, cancellationToken);

    }
}
