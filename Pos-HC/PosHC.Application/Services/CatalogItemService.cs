using static PosHC.Application.Validation.BusinessRules;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHC.Application.Services
{
    public class CatalogItemService : ICatalogItemService
    {

        private readonly IPOSHCRepository _poshsRepository;
        private readonly IClinicStore _store;
        private readonly IAuditService _auditService;

        public CatalogItemService(IPOSHCRepository poshsRepository, IClinicStore store, IAuditService auditService)
        {
            _poshsRepository = poshsRepository;
            _store = store;
            _auditService = auditService;
        }

        public async Task<List<CatalogItemDto>> GetAllCatalogItemsAsync(CancellationToken cancellationToken = default)
        {
            var items = await GetAllCatalogItems(cancellationToken);

            return items.Where(x => x.IsActive).Select(CatalogItemDtoMapper.ToDto).ToList();
        }

        public async Task<CatalogItem> GetCatalogItem(Guid catalogItemId, CancellationToken cancellationToken = default)
        {
            var catalogItems = await GetAllCatalogItems(cancellationToken);
            var catalogItem = catalogItems.Find(d => d.Id == catalogItemId);
            if (catalogItem == null)
            {
                throw new Exception("Catalog Item not found.");
            }

            return catalogItem;
        }
        private async Task<List<CatalogItem>> GetAllCatalogItems(CancellationToken cancellationToken = default)
        {
            var catalogItems = await _poshsRepository.GetAllItemsAsync(cancellationToken);
            return catalogItems;
        }
        public async Task<CatalogItem> SaveAsync(Guid id, CatalogItem input, CancellationToken ct)
        {
            var catalogItem = id == Guid.Empty ? new CatalogItem { Id = Guid.NewGuid() } : await _store.Find<CatalogItem>(x => x.Id == id, ct) ?? throw new BusinessException("Service not found.", 404);
            catalogItem.Name = Required(input.Name, "Name");
            Check(input.UnitPrice >= 0 && input.UnitPrice <= 1_000_000 && decimal.Round(input.UnitPrice, 2) == input.UnitPrice, "Price must be a valid USD amount.");
            Check(Enum.IsDefined(input.Type), "Select a valid item type.");
            catalogItem.Type = input.Type;
            catalogItem.UnitPrice = input.UnitPrice;
            catalogItem.IsActive = input.IsActive;
            if (id == Guid.Empty)
            {
                _store.Add(catalogItem);
            }

            _auditService.Record(id == Guid.Empty ? "Create" : "Update", "CatalogItem", catalogItem.Id);
            await _store.Save(ct);
            return catalogItem;
        }

        public Task<PagedResult<CatalogItem>> GetPageAsync(string search, int page, CancellationToken cancellationToken, int pageSize = 50) => PagedQuery.ReadAsync<CatalogItem>(_store, item => item.Name.Contains(search), page, cancellationToken, pageSize);

    }
}
