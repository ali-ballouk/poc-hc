using System.Linq.Expressions;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHC.Application.Services;

internal static class PagedQuery
{
    public static async Task<PagedResult<T>> ReadAsync<T>(IClinicStore store,
        Expression<Func<T, bool>>? filter, int page, CancellationToken cancellationToken) where T : class
    {
        const int pageSize = 50;
        var skip = (Math.Clamp(page, 1, 100000) - 1) * pageSize;
        var items = await store.List(filter, pageSize, skip, cancellationToken);
        var total = await store.Count(filter, cancellationToken);
        return new PagedResult<T>(items, total, pageSize);
    }
}
