using PosHC.Application.DTOs;
using PosHC.Domain.Entities;

namespace PosHC.Application.Mapping
{
    public static class ClinicDtoMapper
    {
        public static DoctorDetailsDto ToDto(Doctor entity)
        {
            return new DoctorDetailsDto
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Fee = entity.Fee,
                Phone = entity.Phone,
                Specialty = entity.Specialty,
                IsActive = entity.IsActive
            };
        }

        public static PatientDetailsDto ToDto(Patient entity)
        {
            return new PatientDetailsDto
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Phone = entity.Phone,
                Email = entity.Email,
                DateOfBirth = entity.DateOfBirth,
                Address = entity.Address,
                IsActive = entity.IsActive
            };
        }

        public static CatalogItemDetailsDto ToDto(CatalogItem entity)
        {
            return new CatalogItemDetailsDto
            {
                Id = entity.Id,
                Name = entity.Name,
                UnitPrice = entity.UnitPrice,
                Type = entity.Type,
                Settings = entity.Settings,
                IsActive = entity.IsActive
            };
        }

        public static DoctorAvailabilityDetailsDto ToDto(DoctorAvailability entity)
        {
            return new DoctorAvailabilityDetailsDto
            {
                Id = entity.Id,
                DoctorId = entity.DoctorId,
                StartsAt = entity.StartsAt,
                EndsAt = entity.EndsAt
            };
        }

        public static AppointmentDetailsDto ToDto(Appointment entity)
        {
            return new AppointmentDetailsDto
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                DoctorId = entity.DoctorId,
                StartsAt = entity.StartsAt,
                EndsAt = entity.EndsAt,
                Status = entity.Status,
                Reason = entity.Reason
            };
        }

        public static ClinicSettingsDetailsDto ToDto(ClinicSettings entity)
        {
            return new ClinicSettingsDetailsDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Address = entity.Address,
                Phone = entity.Phone,
                TimeZone = entity.TimeZone,
                LbpPerUsd = entity.LbpPerUsd,
                ExchangeRateConfirmed = entity.ExchangeRateConfirmed,
                TaxRate = entity.TaxRate,
                TaxRegistrationNumber = entity.TaxRegistrationNumber,
                SingleDoctorMode = entity.SingleDoctorMode,
                DefaultDoctorId = entity.DefaultDoctorId
            };
        }

        public static StaffUserDetailsDto ToDto(StaffUser entity)
        {
            return new StaffUserDetailsDto
            {
                Id = entity.Id,
                Username = entity.Username,
                DisplayName = entity.DisplayName,
                Role = entity.Role,
                IsActive = entity.IsActive
            };
        }

        public static CashShiftDetailsDto ToDto(CashShift entity)
        {
            return new CashShiftDetailsDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Currency = entity.Currency,
                OpenedAt = entity.OpenedAt,
                ClosedAt = entity.ClosedAt,
                OpeningAmount = entity.OpeningAmount,
                CountedAmount = entity.CountedAmount,
                ExpectedAmount = entity.ExpectedAmount
            };
        }

        public static CashMovementDetailsDto ToDto(CashMovement entity)
        {
            return new CashMovementDetailsDto
            {
                Id = entity.Id,
                CashShiftId = entity.CashShiftId,
                Amount = entity.Amount,
                Reason = entity.Reason,
                CreatedAt = entity.CreatedAt
            };
        }

        public static AuditEntryDetailsDto ToDto(AuditEntry entity)
        {
            return new AuditEntryDetailsDto
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                Actor = entity.Actor,
                Action = entity.Action,
                Entity = entity.Entity,
                EntityId = entity.EntityId,
                Details = entity.Details
            };
        }

        public static PaymentDetailsDto ToDto(Payment entity)
        {
            return new PaymentDetailsDto
            {
                Id = entity.Id,
                Amount = entity.Amount,
                Currency = entity.Currency,
                Kind = entity.Kind,
                Reference = entity.Reference,
                RequestId = entity.RequestId,
                CashShiftId = entity.CashShiftId,
                InvoiceId = entity.InvoiceId,
                PaymentTypeId = entity.PaymentTypeId,
                Settings = entity.Settings,
                PaymentDate = entity.PaymentDate
            };
        }

        public static CreditNoteDetailsDto ToDto(CreditNote entity)
        {
            return new CreditNoteDetailsDto
            {
                Id = entity.Id,
                InvoiceId = entity.InvoiceId,
                Amount = entity.Amount,
                Reason = entity.Reason,
                CreatedAt = entity.CreatedAt,
                RequestId = entity.RequestId
            };
        }

        public static InvoiceDetailsDto ToDto(Invoice entity)
        {
            return new InvoiceDetailsDto
            {
                Id = entity.Id,
                Number = entity.Number,
                Status = entity.Status,
                Currency = entity.Currency,
                ExchangeRate = entity.ExchangeRate,
                TaxRate = entity.TaxRate,
                PatientName = entity.PatientName,
                DoctorName = entity.DoctorName,
                ClinicName = entity.ClinicName,
                ClinicAddress = entity.ClinicAddress,
                ClinicPhone = entity.ClinicPhone,
                RequestId = entity.RequestId,
                DoctorId = entity.DoctorId,
                PatientId = entity.PatientId,
                Discount = entity.Discount,
                DoctorFee = entity.DoctorFee,
                CreatedAt = entity.CreatedAt,
                Subtotal = entity.Subtotal,
                Tax = entity.Tax,
                Total = entity.Total,
                Items = entity.Items.Select(ToDto).ToList()
            };
        }

        public static InvoiceLineDto ToDto(InvoiceItem entity)
        {
            return new InvoiceLineDto
            {
                Id = entity.Id,
                InvoiceId = entity.InvoiceId,
                CatalogItemId = entity.CatalogItemId,
                Description = entity.Description,
                Name = entity.Name,
                Quantity = entity.Quantity,
                UnitPrice = entity.UnitPrice,
                LineTotal = entity.LineTotal
            };
        }

        public static PagedResult<TDto> MapPage<TEntity, TDto>(PagedResult<TEntity> page, Func<TEntity, TDto> map)
        {
            return new PagedResult<TDto>(page.Items.Select(map).ToList(), page.Total, page.PageSize, page.Page);
        }
    }
}
