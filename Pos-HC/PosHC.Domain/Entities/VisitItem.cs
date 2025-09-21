using System.Text.Json;

namespace PosHC.Domain.Entities
{
    public abstract class VisitItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal UnitPrice { get; set; }

        public string Settings { get; set; } = "{}";

        public abstract object SettingsValue { get; set; }

        public void SaveSettings(object settings)
        {
            SettingsValue = JsonSerializer.Serialize(settings);
        }

        protected T DeserializeSettings<T>()
        {
            return JsonSerializer.Deserialize<T>(Settings)!;
        }
    }

    public class ProductItem : VisitItem
    {
        public override object SettingsValue
        {
            get => DeserializeSettings<ProductSettings>();
            set => SaveSettings(value);
        }
    }

    public class ServiceItem : VisitItem
    {
        public override object SettingsValue
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
