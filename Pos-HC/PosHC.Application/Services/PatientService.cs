using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

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
            var doctors = await _poshsRepository.GetAllPatientsAsync(cancellationToken);

            return doctors
                .Select(d => new PatientLookupDto
                {
                    Id = d.Id,
                    FullName = string.Format("{0} {1}", d.FirstName, d.LastName)
                })
                .ToList();
        }
    }
}