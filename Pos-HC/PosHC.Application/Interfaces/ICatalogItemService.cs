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


        Task<CatalogItem> SaveAsync(Guid id, CatalogItem input, CancellationToken ct);
        Task<PagedResult<CatalogItem>> GetPageAsync(string search, int page, CancellationToken cancellationToken, int pageSize = 50);

    }
}
