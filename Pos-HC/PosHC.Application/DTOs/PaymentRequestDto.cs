using System.Text.Json.Serialization;

namespace PosHC.Application.DTOs
{
    public class PaymentTypeLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }


    public class PaymentRequestDto 
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public PaymentSettings Settings { get; set; } = null!;
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "paymentType")]
    [JsonDerivedType(typeof(CashPaymentSettings), "cash")]
    [JsonDerivedType(typeof(CardPaymentSettings), "card")]
    [JsonDerivedType(typeof(OnAccountPaymentSettings), "on-account")]
    [JsonDerivedType(typeof(TransferPaymentSettings), "transfer")]
    public abstract class PaymentSettings { }

    public class CashPaymentSettings : PaymentSettings
    {
        public string CashDrawerId { get; set; } = null!;
    }

    public class CardPaymentSettings : PaymentSettings
    {
        public string CardNumber { get; set; } = null!;
        public string Expiry { get; set; } = null!;
        public string Token { get; set; } = null!;
    }

    public class OnAccountPaymentSettings : PaymentSettings
    {
        public string AccountId { get; set; } = null!;
    }

    public class TransferPaymentSettings : PaymentSettings
    {
        public string Banke { get; set; } = null!;

        public string ReferenceNumber { get; set; } = null!;
    }


}