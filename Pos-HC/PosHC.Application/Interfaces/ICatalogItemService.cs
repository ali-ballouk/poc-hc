using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHC.Application.Interfaces
{
    public interface ICatalogItemService
    {
        Task<List<CatalogItemDto>> GetAllItems(CancellationToken cancellationToken = default);

    }
}