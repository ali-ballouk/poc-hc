using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
namespace PosHC.Application.Services
{
    public class PatientService : IPatientService
    {

        private readonly IPOSHCRepository _poshsRepository;

        public PatientService(IPOSHCRepository doctorRepository)
        {
            _poshsRepository = doctorRepository;
        }

        public async Task<List<PatientLookupDto>> GetAllPatientInfo(CancellationToken cancellationToken = default)
        {
            var patients = await GetAllPatients(cancellationToken);
            return patients.Select(PatientLookupDtoMapper).ToList();
        }


        public PatientLookupDto GetPatient(Guid patientId)
        {
            var patient = GetDoctorAsync(patientId).GetAwaiter().GetResult();
            return PatientLookupDtoMapper(patient);
        }

        private async Task<Patient> GetDoctorAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            var patients = await GetAllPatients();
            var patient = patients.Find(p => p.Id == patientId);
            if (patient == null)
                throw new Exception("Doctor not found.");
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

    }
}