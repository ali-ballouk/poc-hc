using PosHC.Domain.Entities;

namespace PosHC.Application.DTOs
{
    public class CashShiftDetailsDto
    {
        public Guid Id
        {
            get; set;
        }
        public Guid UserId
        {
            get; set;
        }
        public string Currency { get; set; } = "";
        public DateTime OpenedAt
        {
            get; set;
        }
        public DateTime? ClosedAt
        {
            get; set;
        }
        public decimal OpeningAmount
        {
            get; set;
        }
        public decimal? CountedAmount
        {
            get; set;
        }
        public decimal? ExpectedAmount
        {
            get; set;
        }
    }
}
