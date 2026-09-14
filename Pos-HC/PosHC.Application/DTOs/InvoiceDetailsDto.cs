using PosHC.Domain.Entities;

namespace PosHC.Application.DTOs
{
    public class InvoiceDetailsDto
    {
        public Guid Id
        {
            get; set;
        }
        public long Number
        {
            get; set;
        }
        public string Status { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal ExchangeRate
        {
            get; set;
        }
        public decimal TaxRate
        {
            get; set;
        }
        public string PatientName { get; set; } = "";
        public string DoctorName { get; set; } = "";
        public string ClinicName { get; set; } = "";
        public string ClinicAddress { get; set; } = "";
        public string ClinicPhone { get; set; } = "";
        public Guid? RequestId
        {
            get; set;
        }
        public Guid DoctorId
        {
            get; set;
        }
        public Guid PatientId
        {
            get; set;
        }
        public decimal? Discount
        {
            get; set;
        }
        public decimal DoctorFee
        {
            get; set;
        }
        public DateTime CreatedAt
        {
            get; set;
        }
        public decimal Subtotal
        {
            get; set;
        }
        public decimal Tax
        {
            get; set;
        }
        public decimal Total
        {
            get; set;
        }
        public List<InvoiceLineDto> Items { get; set; } = new();
    }
}
