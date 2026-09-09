using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Application.Services;
using PosHC.Domain.Entities;

namespace PosHCExternal.web.Controllers;
[ApiController, Route("api/billing"), Authorize(Roles = "Administrator,Cashier,Receptionist")]
public class BillingController(BillingService billing, IClinicStore store, IReceiptPdfGenerator receipts) : ControllerBase
{
    [HttpGet("invoices")]
    public async Task<object> Invoices(string search = "", Guid? patientId = null, int page = 1, CancellationToken ct = default)
    {
        System.Linq.Expressions.Expression<Func<Invoice, bool>> filter = x => (!patientId.HasValue || x.PatientId == patientId) && (x.PatientName.Contains(search) || x.DoctorName.Contains(search) || x.Number.ToString().Contains(search));
        var rows = await store.List(filter, 50, (Math.Clamp(page, 1, 100000) - 1) * 50, ct);
        var items = new List<object>();
        foreach (var row in rows)
        {
            var b = await billing.Balance(row.Id, ct);
            items.Add(new
            {
                row.Id,
                row.Number,
                row.CreatedAt,
                row.PatientName,
                row.DoctorName,
                row.Currency,
                row.Status,
                row.Total,
                b.Paid,
                b.Credits,
                b.Balance,
                b.PaymentStatus
            });
        }
        return new
        {
            Items = items,
            Total = await store.Count(filter, ct),
            PageSize = 50
        };
    }
    [HttpGet("invoices/{id:guid}")]
    public async Task<object> Details(Guid id, CancellationToken ct) => new { Summary = await billing.Balance(id, ct), Payments = await store.List<Payment>(x => x.InvoiceId == id, int.MaxValue, ct: ct), CreditNotes = await store.List<CreditNote>(x => x.InvoiceId == id, int.MaxValue, ct: ct) };
    [HttpGet("payments/{id:guid}/receipt")]
    public async Task<IActionResult> Receipt(Guid id, CancellationToken ct)
    {
        var payment = await store.Find<Payment>(x => x.Id == id, ct);
        if (payment == null)
        {
            return NotFound();
        }

        return File(receipts.Generate(payment, await billing.Invoice(payment.InvoiceId, ct)), "application/pdf", $"Receipt-{id}.pdf");
    }
    [HttpPost("invoices/{id:guid}/status"), Authorize(Roles = "Administrator,Cashier")]
    public Task<Invoice> Status(Guid id, StatusInput input, CancellationToken ct) => billing.ChangeStatus(id, input.Status, input.Reason, ct);
    [HttpPost("payments"), Authorize(Roles = "Administrator,Cashier")]
    public Task<Payment> Collect(PaymentRequestDto input, CancellationToken ct) => billing.Collect(input, ct);
    [HttpPost("invoices/{id:guid}/credits"), Authorize(Roles = "Administrator")]
    public Task<CreditNote> Credit(Guid id, AdjustmentInput input, CancellationToken ct) => billing.Credit(id, input.Amount, input.Reason, input.RequestId, ct);
    [HttpPost("invoices/{id:guid}/refunds"), Authorize(Roles = "Administrator,Cashier")]
    public Task<Payment> Refund(Guid id, AdjustmentInput input, CancellationToken ct) => billing.Refund(id, input.Amount, input.PaymentTypeId, input.Reason, input.RequestId, ct);
    public record StatusInput(string Status, string Reason = "");
    public record AdjustmentInput(decimal Amount, string Reason, Guid RequestId, int PaymentTypeId = 1);
}
