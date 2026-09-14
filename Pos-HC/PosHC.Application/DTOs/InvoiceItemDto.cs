namespace PosHC.Application.DTOs
{
    public class InvoiceItemDto
    {
        public Guid CatalogItemId
        {
            get; set;
        }
        public int Quantity { get; set; } = 1;
    }
}
