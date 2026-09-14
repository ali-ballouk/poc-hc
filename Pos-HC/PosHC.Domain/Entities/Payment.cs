using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosHC.Domain.Entities
{
    // PosHC.Domain/Entities/Payments/Payment.cs


    public class Payment
    {
        public Guid Id
        {
            get; set;
        }
        public decimal Amount
        {
            get; set;
        }
        public string Currency { get; set; } = "USD";
        public string Kind { get; set; } = "Payment";
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
        public Invoice Invoice { get; set; } = null!;

        public int PaymentTypeId
        {
            get; set;
        }
        public PaymentType PaymentType { get; set; } = null!;

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string Settings
        {
            get; set;
        }
    }

}
