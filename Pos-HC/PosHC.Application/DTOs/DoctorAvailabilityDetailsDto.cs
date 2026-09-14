using PosHC.Domain.Entities;

namespace PosHC.Application.DTOs
{
    public class DoctorAvailabilityDetailsDto
    {
        public Guid Id
        {
            get; set;
        }
        public Guid DoctorId
        {
            get; set;
        }
        public DateTime StartsAt
        {
            get; set;
        }
        public DateTime EndsAt
        {
            get; set;
        }
    }
}
