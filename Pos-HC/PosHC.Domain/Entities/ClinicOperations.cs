namespace PosHC.Domain.Entities;

public class StaffUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Receptionist";
    public bool IsActive { get; set; } = true;
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();
    public int FailedAttempts
    {
        get; set;
    }
    public DateTime? LockedUntil
    {
        get; set;
    }
    public string? ResetTokenHash
    {
        get; set;
    }
    public DateTime? ResetExpires
    {
        get; set;
    }
}

public class ClinicSettings
{
    public int Id { get; set; } = 1;
    public string Name { get; set; } = "POS HC";
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

public class AuditEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Actor { get; set; } = "";
    public string Action { get; set; } = "";
    public string Entity { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string Details { get; set; } = "";
}

public class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId
    {
        get; set;
    }
    public Guid DoctorId
    {
        get; set;
    }
    public DateTime StartsAt
    {
        get; set;
    }
    public DateTime EndsAt
    {
        get; set;
    }
    public string Status { get; set; } = "Booked";
    public string Reason { get; set; } = "";
}

public class DoctorAvailability
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DoctorId
    {
        get; set;
    }
    public DateTime StartsAt
    {
        get; set;
    }
    public DateTime EndsAt
    {
        get; set;
    }
}

public class CashShift
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId
    {
        get; set;
    }
    public string Currency { get; set; } = "USD";
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt
    {
        get; set;
    }
    public decimal OpeningAmount
    {
        get; set;
    }
    public decimal? CountedAmount
    {
        get; set;
    }
    public decimal? ExpectedAmount
    {
        get; set;
    }
}

public class CashMovement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CashShiftId
    {
        get; set;
    }
    public decimal Amount
    {
        get; set;
    }
    public string Reason { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreditNote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId
    {
        get; set;
    }
    public decimal Amount
    {
        get; set;
    }
    public string Reason { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid RequestId
    {
        get; set;
    }
}
