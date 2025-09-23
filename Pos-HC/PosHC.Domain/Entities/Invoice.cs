using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosHC.Domain.Entities
{
    public class Invoice
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public decimal? Discount { get; set; }
        public decimal DoctorFee { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Doctor Doctor { get; set; } = null!;

        public Patient Patient { get; set; } = null!;
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

        // Not mapped: Computed total
        public decimal Subtotal => DoctorFee + Items.Sum(i => i.Quantity * i.UnitPrice);
        public decimal Total => Subtotal - (Discount ?? 0);
    }

   
}
