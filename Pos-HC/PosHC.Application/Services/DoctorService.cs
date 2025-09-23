using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

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
            var doctors = await GetAllDoctorAsync();

            return doctors.Select(DoctorLookupDtoMapper).ToList();
        }

        
        public DoctorLookupDto GetDoctor(Guid doctorId)
        {
            var doctor = GetDoctorAsync(doctorId).GetAwaiter().GetResult();
            return DoctorLookupDtoMapper(doctor);
        }

        private async Task<Doctor> GetDoctorAsync(Guid doctorId, CancellationToken cancellationToken = default)
        {
            var doctors = await GetAllDoctorAsync();
            var doctor = doctors.Find(d => d.Id == doctorId);
            if (doctor == null)
                throw new Exception("Doctor not found.");
            return doctor;
        }
        private async Task<List<Doctor>> GetAllDoctorAsync(CancellationToken cancellationToken = default)
        {
            var doctors = await _poshsRepository.GetAllDoctorsAsync(cancellationToken);
            return doctors;
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
    }
}