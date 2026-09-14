using PosHC.Domain.Entities;

namespace PosHC.Application.DTOs
{
    public class DoctorDetailsDto
    {
        public Guid Id
        {
            get; set;
        }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public decimal Fee
        {
            get; set;
        }
        public string Phone { get; set; } = "";
        public string Specialty { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}
