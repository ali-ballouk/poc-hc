using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosHC.Domain.Entities
{
    // PosHC.Domain/Entities/Payments/Payment.cs
    using System.Text.Json.Serialization;


    public class Payment
    {
        public Guid Id { get; set; }

        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        public int PaymentTypeId { get; set; }
        public PaymentType PaymentType { get; set; } = null!;

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string Settings { get; set; }
    }

}
