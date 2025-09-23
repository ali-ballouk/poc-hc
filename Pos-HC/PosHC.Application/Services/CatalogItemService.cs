using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHC.Application.Services
{
    public class CatalogItemService : ICatalogItemService
    {

        private readonly IPOSHCRepository _poshsRepository;

        public CatalogItemService(IPOSHCRepository poshsRepository)
        {
            _poshsRepository = poshsRepository;
        }

        public async Task<List<CatalogItemDto>> GetAllCatalogItemsAsync(CancellationToken cancellationToken = default)
        {
            var Items = await GetAllcatalogItems(cancellationToken);

            return Items.Select(CatalogItemDtoMapper.ToDto).ToList();
        }

        public async Task<CatalogItem> GetCatalogItem(Guid catalogItemId, CancellationToken cancellationToken = default)
        {
            var catalogItems = await GetAllcatalogItems();
            var catalogItem = catalogItems.Find(d => d.Id == catalogItemId);
            if (catalogItem == null)
                throw new Exception("Catalog Item not found.");
            return catalogItem;
        }
        async Task<List<CatalogItem>> GetAllcatalogItems( CancellationToken cancellationToken = default)
        {
            var catalogItems = await _poshsRepository.GetAllItemsAsync(cancellationToken);
            return catalogItems;
        }
    }
}