using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace PosHC.Domain.Entities
{
    [Table("VisitItem", Schema = "poshc")]
    public abstract class VisitItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal UnitPrice { get; set; }

        public string SettingsJson { get; set; } = "{}";

        public abstract object Settings { get; set; }

        public void SaveSettings(object settings)
        {
            SettingsJson = JsonSerializer.Serialize(settings);
        }

        protected T DeserializeSettings<T>()
        {
            return JsonSerializer.Deserialize<T>(SettingsJson)!;
        }
    }

    public class ProductItem : VisitItem
    {
        public override object Settings
        {
            get => DeserializeSettings<ProductSettings>();
            set => SaveSettings(value);
        }
    }

    public class ServiceItem : VisitItem
    {
        public override object Settings
        {
            get => DeserializeSettings<ServiceSettings>();
            set => SaveSettings(value);
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
