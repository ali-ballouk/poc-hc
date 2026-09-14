using PosHC.Application.Exceptions;
using PosHC.Application.Mapping;
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

        public async Task<PatientDetailsDto> SaveAsync(Guid id, PatientDetailsDto input, CancellationToken ct)
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
            return ClinicDtoMapper.ToDto(patient);
        }

        public async Task<PagedResult<PatientDetailsDto>> GetPageAsync(string search, int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
        {
            var pageResult = await PagedQuery.ReadAsync<Patient>(_store, item => item.FirstName.Contains(search) || item.LastName.Contains(search) || item.Phone.Contains(search), page, cancellationToken, pageSize, sortBy, sortDirection);
            return ClinicDtoMapper.MapPage(pageResult, ClinicDtoMapper.ToDto);
        }

    }
}
