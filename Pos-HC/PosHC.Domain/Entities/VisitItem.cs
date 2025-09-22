using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace PosHC.Domain.Entities
{

    public enum VisitItemType
    {
        Product = 1,
        Service = 2
    }
    public class VisitItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal UnitPrice { get; set; }

        // discriminator
        public VisitItemType Type { get; set; }

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
                    VisitItemType.Product => JsonSerializer.Deserialize<ProductSettings>(Settings),
                    VisitItemType.Service => JsonSerializer.Deserialize<ServiceSettings>(Settings),
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
                    VisitItemType.Product when value is ProductSettings ps =>
                        JsonSerializer.Serialize(ps),
                    VisitItemType.Service when value is ServiceSettings ss =>
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
