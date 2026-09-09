using System.Linq.Expressions;

namespace PosHC.Application.Interfaces;

// Persistence stays behind this interface; application services own workflow rules.
public interface IClinicStore
{
    Task<T?> Find<T>(Expression<Func<T, bool>> predicate, CancellationToken ct = default) where T : class;
    Task<List<T>> List<T>(Expression<Func<T, bool>>? predicate = null, int limit = 500, int skip = 0, CancellationToken ct = default) where T : class;
    Task<int> Count<T>(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default) where T : class;
    void Add<T>(T entity) where T : class;
    Task Save(CancellationToken ct = default);
    Task<T> Transaction<T>(Func<Task<T>> operation, CancellationToken ct = default);
    Task<long> NextInvoiceNumber(CancellationToken ct = default);
}

public interface ICurrentStaff
{
    Guid Id
    {
        get;
    }
    string Name
    {
        get;
    }
    string Role
    {
        get;
    }
}

public class BusinessException(string message, int status = 400) : Exception(message)
{
    public int Status { get; } = status;
}
