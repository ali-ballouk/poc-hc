using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHC.Application.Services
{
    public class DoctorService : IDoctorService
    {

        private readonly IPOSHCRepository _poshsRepository;

        public DoctorService(IPOSHCRepository doctorRepository)
        {
            _poshsRepository = doctorRepository;
        }

        public async Task<List<DoctorLookupDto>> GetAllDoctorInfo(CancellationToken cancellationToken = default)
        {
            var doctors = await _poshsRepository.GetAllDoctorsAsync(cancellationToken);

            return doctors
                .Select(d => new DoctorLookupDto
                {
                    Id = d.Id,
                    FullName = string.Format("{0} {1}", d.FirstName, d.LastName),
                    Fee = d.Fee
                })
                .ToList();
        }
    }
}