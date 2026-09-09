using PosHC.Domain.Entities;
namespace PosHC.Application.Interfaces;
public interface IReceiptPdfGenerator
{
    byte[] Generate(Payment payment, Invoice invoice, string language = "en");
}
