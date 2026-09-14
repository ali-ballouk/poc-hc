using PosHC.Domain.Entities;

namespace PosHC.Application.DTOs
{
    public class PaymentDetailsDto
    {
        public Guid Id
        {
            get; set;
        }
        public decimal Amount
        {
            get; set;
        }
        public string Currency { get; set; } = "";
        public string Kind { get; set; } = "";
        public string Reference { get; set; } = "";
        public Guid? RequestId
        {
            get; set;
        }
        public Guid? CashShiftId
        {
            get; set;
        }
        public Guid InvoiceId
        {
            get; set;
        }
        public int PaymentTypeId
        {
            get; set;
        }
        public DateTime PaymentDate
        {
            get; set;
        }
        public string Settings { get; set; } = "{}";
    }
}
