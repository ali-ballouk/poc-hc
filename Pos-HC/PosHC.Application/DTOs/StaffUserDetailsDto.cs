using PosHC.Domain.Entities;

namespace PosHC.Application.DTOs
{
    public class StaffUserDetailsDto
    {
        public Guid Id
        {
            get; set;
        }
        public string Username { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsActive
        {
            get; set;
        }
    }
}
