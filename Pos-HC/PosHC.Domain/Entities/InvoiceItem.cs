using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosHC.Domain.Entities
{
    public class InvoiceItem
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid CatalogItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }

        public Invoice Invoice { get; set; } = null!;
        public CatalogItem CatalogItem { get; set; } = null!;

        // Not mapped: Computed line total
        public decimal LineTotal => Quantity * UnitPrice;
    }

    public class InvoiceItemDto
    {
        public Guid CatalogItemId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
