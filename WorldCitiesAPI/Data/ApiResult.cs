using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace WorldCitiesAPI.Data;

public class ApiResult<T>
{
    public List<T> Data { get; private set; }
    public int TotalCount { get; private set; }
    public int PageSize { get; private set; }
    public int PageIndex { get; private set; }
    public string? SortColumn { get; set; }
    public string? SortOrder { get; set; }
    public string? FilterColumn { get; set; }
    public string? FilterQuery { get; set; }
    public int TotalPages { get; private set; }
    public bool HasPreviousPage => PageIndex > 0;
    
    public bool HasNextPage => PageIndex + 1 < TotalPages;
    
    private ApiResult(List<T> data, int count, int pageSize, int pageIndex,
        string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
    {
        Data = data;
        TotalCount = count;
        PageSize = pageSize;
        PageIndex = pageIndex;
        SortColumn = sortColumn;
        SortOrder = sortOrder;
        FilterColumn = filterColumn;
        FilterQuery = filterQuery;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }

    public static async Task<ApiResult<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize,
        string? sortColumn = null, string? sortOrder = null, string? filterColumn = null, string? filterQuery = null)
    {

        if(!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterQuery) && IsValidProperty(filterColumn))
            source = source.Where(string.Format("{0}.StartsWith(@0)", filterColumn), filterQuery);
        
        var count = await source.CountAsync();

        if (!string.IsNullOrEmpty(sortColumn) && IsValidProperty(sortColumn))
        {
            sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC"
                ? "ASC"
                : "DESC";

            source = source.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
        }

        source = source.Skip(pageIndex * pageSize).Take(pageSize);

        var data = await source.ToListAsync();

        return new ApiResult<T>(data, count, pageSize, pageIndex, sortColumn, sortOrder, filterColumn, filterQuery);
    }

    public static bool IsValidProperty(string property, bool throwExceptionIfNotFound = true)
    {
        var prop = typeof(T).GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (prop == null && throwExceptionIfNotFound)
            throw new NotSupportedException($"ERROR: Property '{property}' does not exist");

        return prop != null;
    }
}