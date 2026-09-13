using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using static PosHC.Application.Validation.BusinessRules;

namespace PosHC.Application.Services;

internal static class VisitNotes
{
    public static void Apply(Invoice invoice, VisitNotesInput input, ICurrentStaff staff)
    {
        Check(staff.Role is "Administrator" or "Receptionist" or "Doctor", "You do not have permission to edit visit notes.");
        Check((input.VisitDescription?.Length ?? 0) <= 4000, "Visit description must be at most 4000 characters.");
        Check((input.Diagnosis?.Length ?? 0) <= 2000, "Diagnosis must be at most 2000 characters.");
        invoice.VisitDescription = string.IsNullOrWhiteSpace(input.VisitDescription) ? null : input.VisitDescription.Trim();
        invoice.Diagnosis = string.IsNullOrWhiteSpace(input.Diagnosis) ? null : input.Diagnosis.Trim();
        invoice.VisitNotesUpdatedAt = DateTime.UtcNow;
        invoice.VisitNotesUpdatedBy = staff.Name;
    }
}
