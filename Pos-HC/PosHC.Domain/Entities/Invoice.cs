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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Doctor Doctor { get; set; } = null!;

        public Patient Patient { get; set; } = null!;
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

        // Not mapped: Computed total
        public decimal Subtotal => (Doctor?.Fee ?? 0) + Items.Sum(i => i.Quantity * i.UnitPrice);
        public decimal Total => Subtotal - (Discount ?? 0);
    }

    public class InvoiceItem
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid ItemTypeId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }

        public Invoice Invoice { get; set; } = null!;
        public CatalogItem ItemType { get; set; } = null!;

        // Not mapped: Computed line total
        public decimal LineTotal => Quantity * UnitPrice;
    }
}
