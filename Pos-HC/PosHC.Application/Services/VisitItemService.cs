using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHC.Application.Services
{
    public class VisitItemService : IVisitItemService
    {

        private readonly IPOSHCRepository _poshsRepository;

        public VisitItemService(IPOSHCRepository poshsRepository)
        {
            _poshsRepository = poshsRepository;
        }

        public async Task<List<VisitItemDto>> GetAllVisitItems(CancellationToken cancellationToken = default)
        {
            var visitItems = await _poshsRepository.GetAllVisitItemsAsync(cancellationToken);

            return visitItems.Select(VisitItemMapper.ToDto).ToList();
        }
    }
}