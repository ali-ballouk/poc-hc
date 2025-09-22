using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHC.Application.Interfaces
{
    public interface IItemService
    {
        Task<List<ItemDto>> GetAllItems(CancellationToken cancellationToken = default);

    }
}