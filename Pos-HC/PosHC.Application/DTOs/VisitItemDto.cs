using PosHC.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace PosHC.Application.DTOs
{
    public class VisitItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public VisitItemType Type { get; set; }
        public object Settings { get; set; }
    }


    public static class VisitItemMapper
    {
        public static VisitItemDto ToDto(VisitItem entity)
        {
            var dto = new VisitItemDto
            {
                Id = entity.Id,
                Name = entity.Name,
                UnitPrice = entity.UnitPrice,
                Type = entity.Type
            };

            switch (entity.Type)
            {
                case VisitItemType.Product:
                    dto.Settings = JsonSerializer.Deserialize<ProductSettings>(entity.Settings);
                    break;
                case VisitItemType.Service:
                    dto.Settings = JsonSerializer.Deserialize<ServiceSettings>(entity.Settings);
                    break;
            }

            return dto;
        }

        public static VisitItem ToEntity(VisitItemDto dto)
        {
            var entity = new VisitItem
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
