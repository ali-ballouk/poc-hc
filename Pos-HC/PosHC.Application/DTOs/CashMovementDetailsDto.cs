using PosHC.Domain.Entities;

namespace PosHC.Application.DTOs
{
    public class CashMovementDetailsDto
    {
        public Guid Id
        {
            get; set;
        }
        public Guid CashShiftId
        {
            get; set;
        }
        public decimal Amount
        {
            get; set;
        }
        public string Reason { get; set; } = "";
        public DateTime CreatedAt
        {
            get; set;
        }
    }
}
