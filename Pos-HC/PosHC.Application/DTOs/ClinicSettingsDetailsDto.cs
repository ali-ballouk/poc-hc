using PosHC.Domain.Entities;

namespace PosHC.Application.DTOs
{
    public class ClinicSettingsDetailsDto
    {
        public int Id { get; set; } = 1;
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
        public string TimeZone { get; set; } = "Asia/Beirut";
        public decimal LbpPerUsd { get; set; } = 1;
        public bool ExchangeRateConfirmed
        {
            get; set;
        }
        public decimal TaxRate
        {
            get; set;
        }
        public string TaxRegistrationNumber { get; set; } = "";
        public bool SingleDoctorMode
        {
            get; set;
        }
        public Guid? DefaultDoctorId
        {
            get; set;
        }
    }
}
