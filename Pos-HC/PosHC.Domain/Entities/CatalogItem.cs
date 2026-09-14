namespace PosHC.Domain.Entities
{

    public enum ItemType
    {
        Product = 1,
        Service = 2
    }
    public class CatalogItem
    {
        public Guid Id
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }
        public decimal UnitPrice
        {
            get; set;
        }
        public bool IsActive { get; set; } = true;

        // discriminator
        public ItemType Type
        {
            get; set;
        }

        // raw JSON stored in SQL
        public string Settings { get; set; } = "{}";

    }


    public class ProductSettings
    {
        public string Name { get; set; } = "";
    }

    public class ServiceSettings
    {
        public string Duration { get; set; } = "";
        public string Specialty { get; set; } = "";
    }
}
