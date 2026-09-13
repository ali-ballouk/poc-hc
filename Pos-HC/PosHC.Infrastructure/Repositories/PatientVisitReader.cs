using Microsoft.EntityFrameworkCore;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Infrastructure.Persistence;

namespace PosHC.Infrastructure.Repositories;

public class PatientVisitReader(ApplicationDbContext db) : IPatientVisitReader
{
    public async Task<PagedResult<PatientVisitDto>> ReadAsync(Guid patientId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Invoice.AsNoTracking().Where(i => i.PatientId == patientId);
        var total = await query.CountAsync(ct);
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)));
        var rows = await query.OrderByDescending(i => i.CreatedAt).ThenByDescending(i => i.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(i => new PatientVisitDto(i.Id, i.Number, i.CreatedAt, i.DoctorName, i.Status,
                i.VisitDescription, i.Diagnosis, i.VisitNotesUpdatedAt, i.VisitNotesUpdatedBy)).ToListAsync(ct);
        return new PagedResult<PatientVisitDto>(rows, total, pageSize, page);
    }
}
