using PosHC.Application.Exceptions;
using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PosHC.Application.Interfaces;
using PosHC.Infrastructure.Persistence;

namespace PosHC.Infrastructure.Repositories;

public class ClinicStore(ApplicationDbContext db) : IClinicStore
{
    public Task<T?> Find<T>(Expression<Func<T, bool>> predicate, CancellationToken ct = default) where T : class => db.Set<T>().FirstOrDefaultAsync(predicate, ct);
    public Task<List<T>> List<T>(Expression<Func<T, bool>>? predicate = null, int limit = 500, int skip = 0, CancellationToken ct = default, string? sortBy = null, string? sortDirection = null) where T : class
    {
        var query = db.Set<T>().AsQueryable();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if ((sortBy is "DoctorName" or "PatientName" && typeof(T) == typeof(PosHC.Domain.Entities.Appointment)) ||
            (sortBy == "DoctorName" && typeof(T) == typeof(PosHC.Domain.Entities.DoctorAvailability)))
        {
            if (sortDirection is not null && sortDirection != "asc" && sortDirection != "desc")
            {
                throw new BusinessException("Sort direction must be asc or desc.");
            }

            Expression<Func<T, string?>> name = sortBy == "DoctorName"
                ? row => db.Set<PosHC.Domain.Entities.Doctor>().Where(d => d.Id == EF.Property<Guid>(row, "DoctorId")).Select(d => d.FirstName + " " + d.LastName).FirstOrDefault()
                : row => db.Set<PosHC.Domain.Entities.Patient>().Where(p => p.Id == EF.Property<Guid>(row, "PatientId")).Select(p => p.FirstName + " " + p.LastName).FirstOrDefault();
            return (sortDirection == "desc" ? query.OrderByDescending(name) : query.OrderBy(name))
                .ThenBy(row => EF.Property<Guid>(row, "Id")).Skip(skip).Take(limit).ToListAsync(ct);
        }
        return PosHC.Application.Services.GridOrdering.Apply(query, sortBy, sortDirection).Skip(skip).Take(limit).ToListAsync(ct);
    }
    public Task<int> Count<T>(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default) where T : class => predicate == null ? db.Set<T>().CountAsync(ct) : db.Set<T>().CountAsync(predicate, ct);
    public void Add<T>(T entity) where T : class => db.Add(entity);
    public Task Save(CancellationToken ct = default) => db.SaveChangesAsync(ct);
    public async Task<T> Transaction<T>(Func<Task<T>> operation, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        // Serialize short clinic write workflows before reading balances or slot availability.
        // This avoids shared-lock upgrade deadlocks and protects concurrent first-user setup.
        await db.Database.ExecuteSqlRawAsync("DECLARE @result int; EXEC @result=sp_getapplock @Resource='PosHC.ClinicWrite',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000; IF @result<0 THROW 51000,'Clinic is busy. Retry the operation.',1;", ct);
        var result = await operation();
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return result;
    }
    public async Task<long> NextInvoiceNumber(CancellationToken ct = default) =>
        (await db.Database.SqlQueryRaw<long>("SELECT NEXT VALUE FOR poshc.InvoiceNumber AS Value").ToListAsync(ct)).Single();
}
