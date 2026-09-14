using PosHC.Domain.Entities;

namespace PosHC.Application.DTOs
{
    public class CreditNoteDetailsDto
    {
        public Guid Id
        {
            get; set;
        }
        public Guid InvoiceId
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
        public Guid RequestId
        {
            get; set;
        }
    }
}
