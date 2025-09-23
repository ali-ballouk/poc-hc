using PosHC.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace PosHC.Application.DTOs
{
    public class CatalogItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public ItemType Type { get; set; }
        public object Settings { get; set; }
    }


    public static class CatalogItemDtoMapper
    {
        public static CatalogItemDto ToDto(CatalogItem entity)
        {
            var dto = new CatalogItemDto
            {
                Id = entity.Id,
                Name = entity.Name,
                UnitPrice = entity.UnitPrice,
                Type = entity.Type
            };

            switch (entity.Type)
            {
                case ItemType.Product:
                    dto.Settings = JsonSerializer.Deserialize<ProductSettings>(entity.Settings);
                    break;
                case ItemType.Service:
                    dto.Settings = JsonSerializer.Deserialize<ServiceSettings>(entity.Settings);
                    break;
            }

            return dto;
        }

        public static CatalogItem ToEntity(CatalogItemDto dto)
        {
            var entity = new CatalogItem
            {
                Id = dto.Id,
                Name = dto.Name,
                UnitPrice = dto.UnitPrice,
                Type = dto.Type,
                Settings = JsonSerializer.Serialize(dto.Settings)
            };
            return entity;
        }
    }
}
