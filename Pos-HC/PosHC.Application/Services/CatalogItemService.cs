using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHC.Application.Services
{
    public class CatalogItemService : ICatalogItemService
    {

        private readonly IPOSHCRepository _poshsRepository;

        public CatalogItemService(IPOSHCRepository poshsRepository)
        {
            _poshsRepository = poshsRepository;
        }

        public async Task<List<CatalogItemDto>> GetAllItems(CancellationToken cancellationToken = default)
        {
            var Items = await _poshsRepository.GetAllItemsAsync(cancellationToken);

            return Items.Select(CatalogItemDtoMapper.ToDto).ToList();
        }
    }
}