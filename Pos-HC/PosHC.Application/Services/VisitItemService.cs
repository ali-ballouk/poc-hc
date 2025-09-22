using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHC.Application.Services
{
    public class ItemService : IItemService
    {

        private readonly IPOSHCRepository _poshsRepository;

        public ItemService(IPOSHCRepository poshsRepository)
        {
            _poshsRepository = poshsRepository;
        }

        public async Task<List<ItemDto>> GetAllItems(CancellationToken cancellationToken = default)
        {
            var Items = await _poshsRepository.GetAllItemsAsync(cancellationToken);

            return Items.Select(ItemMapper.ToDto).ToList();
        }
    }
}