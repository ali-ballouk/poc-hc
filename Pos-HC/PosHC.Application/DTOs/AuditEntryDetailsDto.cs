using PosHC.Domain.Entities;

namespace PosHC.Application.DTOs
{
    public class AuditEntryDetailsDto
    {
        public Guid Id
        {
            get; set;
        }
        public DateTime CreatedAt
        {
            get; set;
        }
        public string Actor { get; set; } = "";
        public string Action { get; set; } = "";
        public string Entity { get; set; } = "";
        public string EntityId { get; set; } = "";
        public string Details { get; set; } = "";
    }
}
