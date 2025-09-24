using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace PosHC.Domain.Entities
{

    public enum ItemType
    {
        Product = 1,
        Service = 2
    }
    public class CatalogItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }

        // discriminator
        public ItemType Type { get; set; }

        // raw JSON stored in SQL
        public string Settings { get; set; } = "{}";

        [NotMapped]
        public object? SettingsValue
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Settings))
                    return null;

                return Type switch
                {
                    ItemType.Product => JsonSerializer.Deserialize<ProductSettings>(Settings),
                    ItemType.Service => JsonSerializer.Deserialize<ServiceSettings>(Settings),
                    _ => null
                };
            }
            set
            {
                if (value is null)
                {
                    Settings = "{}";
                    return;
                }

                Settings = Type switch
                {
                    ItemType.Product when value is ProductSettings ps =>
                        JsonSerializer.Serialize(ps),
                    ItemType.Service when value is ServiceSettings ss =>
                        JsonSerializer.Serialize(ss),
                    _ => throw new InvalidOperationException($"Type {Type} and value mismatch")
                };
            }
        }
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
