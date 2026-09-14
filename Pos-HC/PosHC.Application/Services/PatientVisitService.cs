using PosHC.Application.Exceptions;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHC.Application.Services
{
    public class PatientVisitService : IPatientVisitService
    {
        private readonly IClinicStore _store;
        private readonly IPatientVisitReader _reader;
        private readonly IAuditService _audit;
        private readonly ICurrentStaff _staff;

        public PatientVisitService(IClinicStore store, IPatientVisitReader reader, IAuditService audit, ICurrentStaff staff)
        {
            _store = store;
            _reader = reader;
            _audit = audit;
            _staff = staff;
        }

        public async Task<PagedResult<PatientVisitDto>> GetPageAsync(Guid patientId, int page, int pageSize, CancellationToken ct, string? sortBy = null, string? sortDirection = null)
        {
            if (await _store.Find<Patient>(p => p.Id == patientId, ct) == null)
            {
                throw new BusinessException("Patient not found.", 404);
            }

            return await _reader.ReadAsync(patientId, page, pageSize, ct, sortBy, sortDirection);
        }

        public Task<PatientVisitDto> UpdateAsync(Guid patientId, Guid visitId, VisitNotesInput input, CancellationToken ct)
            => _store.Transaction(async () =>
            {
                var invoice = await _store.Find<Invoice>(i => i.Id == visitId && i.PatientId == patientId, ct)
                    ?? throw new BusinessException("Patient visit not found.", 404);
                VisitNotes.Apply(invoice, input, _staff);
                _audit.Record("UpdateNotes", "PatientVisit", invoice.Id);
                return new PatientVisitDto(invoice.Id, invoice.Number, invoice.CreatedAt, invoice.DoctorName,
                    invoice.Status, invoice.VisitDescription, invoice.Diagnosis, invoice.VisitNotesUpdatedAt, invoice.VisitNotesUpdatedBy);
            }, ct);
    }
}
