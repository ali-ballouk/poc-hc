using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHC.Application.Services
{
    public class DoctorService : IDoctorService
    {

        private readonly IDoctorRepository _doctorRepository;

        public DoctorService(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public async Task<List<DoctorLookupDto>> GetAllDoctorInfo(CancellationToken cancellationToken = default)
        {
            var doctors = await _doctorRepository.GetAllAsync(cancellationToken);

            return doctors
                .Select(d => new DoctorLookupDto
                {
                    Id = d.Id,
                    FullName = string.Format("{0} {1}", d.FirstName, d.LastName)
                })
                .ToList();
        }
    }
}