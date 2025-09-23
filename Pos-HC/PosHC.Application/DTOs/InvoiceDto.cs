using PosHC.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosHC.Application.DTOs
{

  
    public class CreateInvoiceDto
    {
        public Guid DoctorId { get; set; }

        public Guid PatientId { get; set; }

        public string DoctorName { get; set; }
        public string PatientName { get; set; }

        public decimal? Discount { get; set; }
        public List<InvoiceItemDto> Items { get; set; } = new();
    }

    public class InvoiceDto
    {
        public Guid InvoiceId  { get; set; }

        public DateTime InvoiceDate { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; }

        public Guid PatientId { get; set; }

        public string PatientName { get; set; }

        public decimal? Discount { get; set; }

        public decimal DoctorFee { get; set; }

        public decimal Total { get; set; }


        public List<InvoiceItemDto> Items { get; set; } = new();
    }

    public class InvoiceResultDto
    {
        public Guid InvoiceId { get; set; }
        public decimal Total { get; set; }
    }

}
