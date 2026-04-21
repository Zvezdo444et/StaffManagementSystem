using Inspector.Models;
using Inspector.Models.Search;
using System.Data;

namespace Inspector.Services;

public interface IEmployeeSearchService
{
    Task<IReadOnlyList<Employee>> GetEmployeesForSearchAsync(CancellationToken ct = default);
    Task<DataTable> SearchAsync(SearchTemplate template);
}
