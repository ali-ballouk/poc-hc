using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using static PosHC.Application.Validation.BusinessRules;

namespace PosHC.Application.Services;

public record InvoiceBalance(Invoice Invoice, decimal Paid, decimal Credits, decimal Balance, string PaymentStatus);
public class BillingService(IClinicStore store, IClinicSettingsService settingsService, ICashShiftService cashShiftService, IAuditService auditService, ICurrentStaff staff)
{
    public async Task<Invoice> Invoice(Guid id, CancellationToken ct)
    {
        var invoice = await store.Find<Invoice>(x => x.Id == id, ct) ?? throw new BusinessException("Invoice not found.", 404);
        invoice.Items = await store.List<InvoiceItem>(x => x.InvoiceId == id, int.MaxValue, ct: ct);
        return invoice;
    }
    public async Task<InvoiceBalance> Balance(Guid id, CancellationToken ct)
    {
        var invoice = await Invoice(id, ct);
        var paid = (await store.List<Payment>(x => x.InvoiceId == id, int.MaxValue, ct: ct)).Sum(x => x.Amount);
        var credits = (await store.List<CreditNote>(x => x.InvoiceId == id, int.MaxValue, ct: ct)).Sum(x => x.Amount);
        var balance = invoice.Status == "Void" ? 0 : invoice.Total - credits - paid;
        var paymentStatus = invoice.Status switch
        {
            "Draft" or "Void" => invoice.Status,
            _ when balance < 0 => "RefundDue",
            _ when balance == 0 => "Paid",
            _ when paid > 0 => "PartiallyPaid",
            _ => "Unpaid"
        };
        return new(invoice, paid, credits, balance, paymentStatus);
    }
    public Task<Invoice> Create(CreateInvoiceDto input, CancellationToken ct) => store.Transaction(async () =>
    {
        if (input.RequestId.HasValue && await store.Find<Invoice>(x => x.RequestId == input.RequestId, ct) is { } existing)
        {
            return await Invoice(existing.Id, ct);
        }

        var settings = await settingsService.GetAsync(ct);
        var doctorId = ClinicDoctor.Resolve(settings, input.DoctorId);
        var doctor = await store.Find<Doctor>(x => x.Id == doctorId && x.IsActive, ct) ?? throw new BusinessException("Select an active doctor.");
        var patient = await store.Find<Patient>(x => x.Id == input.PatientId && x.IsActive, ct) ?? throw new BusinessException("Select an active patient.");
        Money(0, input.Currency);
        Check(input.Currency != "LBP" || settings.ExchangeRateConfirmed, "Configure and confirm the LBP/USD exchange rate in clinic settings first.");
        var rate = input.Currency == "LBP" ? settings.LbpPerUsd : 1;
        decimal Convert(decimal usd) => Math.Round(usd * rate, input.Currency == "LBP" ? 0 : 2, MidpointRounding.AwayFromZero);
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            Number = await store.NextInvoiceNumber(ct),
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            PatientName = $"{patient.FirstName} {patient.LastName}",
            DoctorName = $"{doctor.FirstName} {doctor.LastName}",
            DoctorFee = Convert(doctor.Fee),
            Discount = input.Discount ?? 0,
            Currency = input.Currency,
            ExchangeRate = rate,
            TaxRate = settings.TaxRate,
            ClinicName = settings.Name,
            ClinicAddress = settings.Address,
            ClinicPhone = settings.Phone,
            Status = input.Draft ? "Draft" : "Issued",
            CreatedAt = DateTime.UtcNow,
            RequestId = input.RequestId
        };
        if (!string.IsNullOrWhiteSpace(input.VisitDescription) || !string.IsNullOrWhiteSpace(input.Diagnosis))
            VisitNotes.Apply(invoice, new VisitNotesInput(input.VisitDescription, input.Diagnosis), staff);
        Check(input.Items != null && input.Items.Count <= 200, "An invoice may contain at most 200 items.");
        foreach (var item in input.Items!)
        {
            Check(item.Quantity > 0 && item.Quantity <= 10000, "Quantity must be between 1 and 10,000.");
            var catalog = await store.Find<CatalogItem>(x => x.Id == item.CatalogItemId && x.IsActive, ct) ?? throw new BusinessException("Select an active service or product.");
            invoice.Items.Add(new InvoiceItem { Id = Guid.NewGuid(), InvoiceId = invoice.Id, CatalogItemId = catalog.Id, Quantity = item.Quantity, UnitPrice = Convert(catalog.UnitPrice), Description = catalog.Name });
        }
        Money(invoice.Discount.Value, invoice.Currency);
        Check(invoice.Discount >= 0 && invoice.Discount <= invoice.Subtotal, "Discount must be between zero and the subtotal.");
        Money(invoice.Total, invoice.Currency);
        store.Add(invoice);
        auditService.Record("Create", "Invoice", invoice.Id, $"{invoice.Number}; {invoice.Currency}; {invoice.Total}");
        return invoice;
    }, ct);
    public Task<Invoice> ChangeStatus(Guid id, string status, string reason, CancellationToken ct) => store.Transaction(async () =>
    {
        var balance = await Balance(id, ct);
        var invoice = balance.Invoice;
        if (status == "Issued")
        {
            Check(invoice.Status == "Draft", "Only draft invoices can be issued.");
        }
        else if (status == "Void")
        {
            Required(reason, "Cancellation reason", 500);
            Check(invoice.Status != "Void" && balance.Paid == 0 && balance.Credits == 0, "Only invoices without collections or credit notes can be voided.");
        }
        else
        {
            throw new BusinessException("Invalid invoice status.");
        }

        invoice.Status = status;
        auditService.Record(status, "Invoice", id, reason);
        return invoice;
    }, ct);
    public Task<Payment> Collect(PaymentRequestDto input, CancellationToken ct) => store.Transaction(async () =>
    {
        if (input.RequestId.HasValue && await store.Find<Payment>(x => x.RequestId == input.RequestId, ct) is { } existing)
        {
            Check(existing.InvoiceId == input.InvoiceId && existing.PaymentTypeId == input.PaymentTypeId, "This request ID belongs to another payment.");
            return existing;
        }
        var balance = await Balance(input.InvoiceId, ct);
        Check(balance.Invoice.Status == "Issued", "Issue the invoice before recording a payment.");
        Check(await store.Find<PaymentType>(x => x.Id == input.PaymentTypeId, ct) != null, "Payment type not found.");
        string reference = input.Reference?.Trim() ?? "";
        Check(input.PaymentTypeId == 1 && input.Settings is CashPaymentSettings, "Only cash payments are supported.");
        var amount = input.Amount ?? balance.Balance;
        Money(amount, balance.Invoice.Currency);
        Check(amount > 0 && amount <= balance.Balance, "Payment must be greater than zero and no more than the outstanding balance.");
        Guid? shiftId = null;
        if (input.PaymentTypeId == 1)
        {
            shiftId = (await store.Find<CashShift>(x => x.UserId == staff.Id && x.Currency == balance.Invoice.Currency && x.ClosedAt == null, ct) ?? throw new BusinessException("Open a cash shift for this currency first.")).Id;
        }

        var row = new Payment
        {
            Id = Guid.NewGuid(),
            InvoiceId = input.InvoiceId,
            PaymentTypeId = input.PaymentTypeId,
            Amount = amount,
            Currency = balance.Invoice.Currency,
            RequestId = input.RequestId,
            Reference = reference,
            CashShiftId = shiftId,
            Settings = "{}",
            PaymentDate = DateTime.UtcNow
        };
        store.Add(row);
        auditService.Record("Collect", "Invoice", input.InvoiceId, $"{amount} {row.Currency}; payment={row.Id}");
        return row;
    }, ct);
    public Task<CreditNote> Credit(Guid id, decimal amount, string reason, Guid requestId, CancellationToken ct) => store.Transaction(async () =>
    {
        Check(requestId != Guid.Empty, "Request ID is required.");
        if (await store.Find<CreditNote>(x => x.RequestId == requestId, ct) is { } existing)
        {
            return existing;
        }

        var balance = await Balance(id, ct);
        Check(balance.Invoice.Status == "Issued", "Only issued invoices can be credited.");
        Money(amount, balance.Invoice.Currency);
        Check(amount > 0 && amount <= balance.Invoice.Total - balance.Credits, "Credit exceeds the remaining invoice value.");
        var row = new CreditNote { InvoiceId = id, Amount = amount, Reason = Required(reason, "Credit reason", 500), RequestId = requestId };
        store.Add(row);
        auditService.Record("Credit", "Invoice", id, $"{amount}; {row.Reason}");
        return row;
    }, ct);
    public Task<Payment> Refund(Guid id, decimal amount, int method, string reason, Guid requestId, CancellationToken ct) => store.Transaction(async () =>
    {
        Check(requestId != Guid.Empty, "Request ID is required.");
        if (await store.Find<Payment>(x => x.RequestId == requestId, ct) is { } existing)
        {
            return existing;
        }

        var balance = await Balance(id, ct);
        Money(amount, balance.Invoice.Currency);
        Check(amount > 0 && amount <= -balance.Balance, "Create a credit note first. Refund cannot exceed the amount due back to the patient.");
        Check(method == 1, "Only cash refunds are supported.");
        Guid? shiftId = null;
        if (method == 1)
        {
            var shift = await store.Find<CashShift>(x => x.UserId == staff.Id && x.Currency == balance.Invoice.Currency && x.ClosedAt == null, ct) ?? throw new BusinessException("Open a cash shift for this currency first.");
            Check(await cashShiftService.Expected(shift, ct) >= amount, "Refund exceeds available cash.");
            shiftId = shift.Id;
        }
        var row = new Payment { Id = Guid.NewGuid(), InvoiceId = id, Amount = -amount, Currency = balance.Invoice.Currency, PaymentTypeId = method, Kind = "Refund", Reference = Required(reason, "Refund reference/reason", 500), Settings = "{}", RequestId = requestId, CashShiftId = shiftId, PaymentDate = DateTime.UtcNow };
        store.Add(row);
        auditService.Record("Refund", "Invoice", id, $"{amount}; {row.Reference}");
        return row;
    }, ct);
}
