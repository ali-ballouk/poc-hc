using PosHC.Application.DTOs;

namespace PosHC.Application.Interfaces
{
    public interface IReceiptPdfGenerator
    {
        byte[] Generate(ReceiptGenerateDto receipt, string language = "en");
    }
}
