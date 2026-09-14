using PosHC.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using System.Linq.Expressions;
using PosHC.Application.Interfaces;
using PosHC.Application.Services;
using PosHC.Domain.Entities;
using PosHCExternal.web.Controllers;

namespace PosHC.Tests;

public class PagingTests
{
    [Fact]
    public async Task Sort_is_forwarded_and_applied_before_paging()
    {
        var store = new PagingStore();
        var controller = new AuditController(new AuditService(store, null!));
        var response = await controller.GetPage("", 1, default, 10, "Actor", "desc");
        var result = Assert.IsType<PagedResult<AuditEntryDetailsDto>>(Assert.IsType<OkObjectResult>(response).Value);
        Assert.All(result.Items, row => Assert.Equal("other", row.Actor));
        Assert.Equal(126, result.Total);
    }

    [Theory]
    [InlineData("PasswordHash", "asc")]
    [InlineData("UnknownColumn", "asc")]
    [InlineData("Username", "invalid")]
    public void Rejects_unsupported_sort_fields_and_directions(string field, string direction)
        => Assert.Throws<BusinessException>(() => GridOrdering.Apply(new List<StaffUser>().AsQueryable(), field, direction));

    [Fact]
    public async Task Controller_forwards_page_size_and_counts_all_matching_records()
    {
        var store = new PagingStore();
        var controller = new AuditController(new AuditService(store, null!));
        var response = await controller.GetPage("match", 2, default, 20);
        var page = Assert.IsType<PagedResult<AuditEntryDetailsDto>>(Assert.IsType<OkObjectResult>(response).Value);
        Assert.Equal(63, page.Total);
        Assert.Equal(20, page.PageSize);
        Assert.Equal(2, page.Page);
        Assert.Equal(20, page.Items.Count);
        Assert.Equal(20, store.LastSkip);
        Assert.Equal(20, store.LastLimit);
        Assert.All(page.Items, row => Assert.Equal("match", row.Actor));
    }

    [Theory]
    [InlineData(999, 20, 4, 20, 60, 3)]
    [InlineData(-1, 20, 1, 20, 0, 20)]
    [InlineData(1, 0, 1, 1, 0, 1)]
    [InlineData(1, 10000, 1, 100, 0, 63)]
    public async Task Clamps_page_and_size_before_querying(int requestedPage, int requestedSize, int pageNumber, int size, int skip, int count)
    {
        var store = new PagingStore();
        var result = await new AuditService(store, null!).GetPageAsync("match", requestedPage, default, requestedSize);
        Assert.Equal(pageNumber, result.Page);
        Assert.Equal(size, result.PageSize);
        Assert.Equal(skip, store.LastSkip);
        Assert.Equal(count, result.Items.Count);
    }

    [Fact]
    public async Task Empty_search_results_return_first_page_and_zero_total()
    {
        var store = new PagingStore();
        var result = await new AuditService(store, null!).GetPageAsync("missing", 10, default, 50);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
        Assert.Equal(1, result.Page);
        Assert.Equal(0, store.LastSkip);
    }

    private sealed class PagingStore : IClinicStore
    {
        private readonly List<AuditEntry> rows = Enumerable.Range(0, 126)
            .Select(i => new AuditEntry { Actor = i % 2 == 0 ? "match" : "other", Entity = "Invoice", Action = "Read" }).ToList();
        public int LastSkip
        {
            get; private set;
        }
        public int LastLimit
        {
            get; private set;
        }
        private IEnumerable<T> Query<T>(Expression<Func<T, bool>>? predicate) where T : class
            => rows.OfType<T>().Where(predicate?.Compile() ?? (_ => true));
        public Task<List<T>> List<T>(Expression<Func<T, bool>>? predicate = null, int limit = 500, int skip = 0, CancellationToken ct = default, string? sortBy = null, string? sortDirection = null) where T : class
        {
            LastSkip = skip;
            LastLimit = limit;
            return Task.FromResult(GridOrdering.Apply(Query(predicate).AsQueryable(), sortBy, sortDirection).Skip(skip).Take(limit).ToList());
        }
        public Task<int> Count<T>(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default) where T : class => Task.FromResult(Query(predicate).Count());
        public Task<T?> Find<T>(Expression<Func<T, bool>> predicate, CancellationToken ct = default) where T : class => Task.FromResult(Query(predicate).FirstOrDefault());
        public void Add<T>(T entity) where T : class => throw new NotSupportedException();
        public Task Save(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<T> Transaction<T>(Func<Task<T>> operation, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<long> NextInvoiceNumber(CancellationToken ct = default) => throw new NotSupportedException();
    }
}
