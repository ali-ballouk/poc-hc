using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;

namespace PosHC.Application.Interfaces
{
    public interface IVisitItemService
    {
        Task<List<VisitItemDto>> GetAllVisitItems(CancellationToken cancellationToken = default);

    }
}