using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using System.Threading.Tasks;

namespace PosHC.Application.Interfaces
{
    public interface ICatalogItemService
    {
        Task<List<CatalogItemDto>> GetAllCatalogItemsAsync(CancellationToken cancellationToken = default);

        Task<CatalogItem> GetCatalogItem(Guid catalogItemId, CancellationToken cancellationToken = default);


        Task<CatalogItemDetailsDto> SaveAsync(Guid id, CatalogItemDetailsDto input, CancellationToken ct);
        Task<PagedResult<CatalogItemDetailsDto>> GetPageAsync(string search, int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null);

    }
}
