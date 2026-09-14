using PosHC.Application.Exceptions;
using System.Linq.Expressions;
using System.Reflection;
using PosHC.Application.DTOs;
using PosHC.Domain.Entities;
using PosHC.Application.Interfaces;

namespace PosHC.Application.Services
{
    public static class GridOrdering
    {
        public static IOrderedQueryable<T> Apply<T>(IQueryable<T> query, string? sortBy, string? sortDirection, string fallback = "Id")
        {
            var key = string.IsNullOrWhiteSpace(sortBy) ? fallback : sortBy;
            var publicType = typeof(T) == typeof(StaffUser) ? typeof(StaffUserDetailsDto) :
                typeof(T) == typeof(Invoice) ? typeof(InvoiceDetailsDto) : typeof(T);
            var publicProperty = publicType.GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var property = typeof(T).GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var type = property == null ? null : Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (property == null || publicProperty == null ||
                !(type!.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(Guid)))
            {
                throw new BusinessException("Unsupported sort column.");
            }

            if (sortDirection is not null && !sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase) && !sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException("Sort direction must be asc or desc.");
            }

            var parameter = Expression.Parameter(typeof(T), "row");
            var lambda = Expression.Lambda(Expression.Property(parameter, property), parameter);
            var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            var expression = Expression.Call(typeof(Queryable), descending ? "OrderByDescending" : "OrderBy", [typeof(T), property.PropertyType], query.Expression, Expression.Quote(lambda));
            var ordered = (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(expression);
            var id = typeof(T).GetProperty("Id");
            if (id == null || property.Name == "Id")
            {
                return ordered;
            }

            var tie = Expression.Lambda(Expression.Property(parameter, id), parameter);
            return (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(Expression.Call(typeof(Queryable), "ThenBy", [typeof(T), id.PropertyType], ordered.Expression, Expression.Quote(tie)));
        }
    }
}
