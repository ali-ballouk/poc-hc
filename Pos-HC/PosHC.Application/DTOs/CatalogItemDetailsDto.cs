using PosHC.Domain.Entities;

namespace PosHC.Application.DTOs
{
    public class CatalogItemDetailsDto
    {
        public Guid Id
        {
            get; set;
        }
        public string Name { get; set; } = "";
        public decimal UnitPrice
        {
            get; set;
        }
        public ItemType Type
        {
            get; set;
        }
        public bool IsActive { get; set; } = true;
        public string Settings { get; set; } = "{}";
    }
}
