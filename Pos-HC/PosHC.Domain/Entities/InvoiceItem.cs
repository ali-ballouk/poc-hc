using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosHC.Domain.Entities
{
    public class InvoiceItem
    {
        public string Description { get; set; } = "";
        public Guid Id
        {
            get; set;
        }
        public Guid InvoiceId
        {
            get; set;
        }
        public Guid CatalogItemId
        {
            get; set;
        }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice
        {
            get; set;
        }

        public Invoice Invoice
        {
            get; set;
        }

        public string Name => string.IsNullOrEmpty(Description) ? CatalogItem?.Name ?? "" : Description;
        public CatalogItem CatalogItem
        {
            get; set;
        }

        // Not mapped: Computed line total
        public decimal LineTotal => Quantity * UnitPrice;
    }

}
