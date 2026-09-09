
namespace PosHC.Domain.Entities
{
    public class Patient
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
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime? DateOfBirth
        {
            get; set;
        }
        public string Address { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}
