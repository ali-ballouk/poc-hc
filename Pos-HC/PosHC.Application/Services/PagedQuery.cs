using System.Linq.Expressions;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHC.Application.Services;

internal static class PagedQuery
{
    public static async Task<PagedResult<T>> ReadAsync<T>(IClinicStore store,
        Expression<Func<T, bool>>? filter, int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null) where T : class
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        var total = await store.Count(filter, cancellationToken);
        var pageCount = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        page = Math.Clamp(page, 1, pageCount);
        var items = await store.List(filter, pageSize, (page - 1) * pageSize, cancellationToken, sortBy, sortDirection);
        return new PagedResult<T>(items, total, pageSize, page);
    }
}
