using PosHC.Domain.Entities;

namespace PosHC.Application.DTOs
{
    public class InvoiceLineDto
    {
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
        public string Description { get; set; } = "";
        public string Name { get; set; } = "";
        public int Quantity
        {
            get; set;
        }
        public decimal UnitPrice
        {
            get; set;
        }
        public decimal LineTotal
        {
            get; set;
        }
    }
}
