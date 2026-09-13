using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHC.Application.Services;

public class PatientVisitService(IClinicStore store, IPatientVisitReader reader, IAuditService audit, ICurrentStaff staff)
{
    public async Task<PagedResult<PatientVisitDto>> GetPageAsync(Guid patientId, int page, int pageSize, CancellationToken ct)
    {
        if (await store.Find<Patient>(p => p.Id == patientId, ct) == null)
            throw new BusinessException("Patient not found.", 404);
        return await reader.ReadAsync(patientId, page, pageSize, ct);
    }

    public Task<PatientVisitDto> UpdateAsync(Guid patientId, Guid visitId, VisitNotesInput input, CancellationToken ct)
        => store.Transaction(async () =>
        {
            var invoice = await store.Find<Invoice>(i => i.Id == visitId && i.PatientId == patientId, ct)
                ?? throw new BusinessException("Patient visit not found.", 404);
            VisitNotes.Apply(invoice, input, staff);
            audit.Record("UpdateNotes", "PatientVisit", invoice.Id);
            return new PatientVisitDto(invoice.Id, invoice.Number, invoice.CreatedAt, invoice.DoctorName,
                invoice.Status, invoice.VisitDescription, invoice.Diagnosis, invoice.VisitNotesUpdatedAt, invoice.VisitNotesUpdatedBy);
        }, ct);
}
