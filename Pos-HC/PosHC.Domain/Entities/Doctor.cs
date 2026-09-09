
namespace PosHC.Domain.Entities
{
    public class Doctor
    {
        public Guid Id
        {
            get; set;
        }
        public string FirstName
        {
            get; set;
        }
        public string LastName
        {
            get; set;
        }
        public decimal Fee
        {
            get; set;
        }
        public string Phone { get; set; } = "";
        public string Specialty { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}
