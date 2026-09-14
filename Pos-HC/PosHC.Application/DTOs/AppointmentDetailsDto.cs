using PosHC.Domain.Entities;

namespace PosHC.Application.DTOs
{
    public class AppointmentDetailsDto
    {
        public Guid Id
        {
            get; set;
        }
        public Guid PatientId
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
        public string Status { get; set; } = "Booked";
        public string Reason { get; set; } = "";
    }
}
