namespace PosHC.Application.DTOs;

public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int PageSize = 50, int Page = 1);
