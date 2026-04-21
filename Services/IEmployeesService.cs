using Inspector.Data;
using Inspector.Models;

namespace Inspector.Services;

public interface IEmployeesService
{
    Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken ct = default);
    Task<Employee> AddAsync(Employee employee, CancellationToken ct = default);
    Task<IReadOnlyList<RelationType>> GetRelationTypesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<EducationLevel>> GetEducationLevelsAsync(CancellationToken ct = default);
    Task<InspectorDbContext> GetDbContextAsync();
    Task<Employee> UpdateAsync(Employee employee, CancellationToken ct = default);
    Task<Employee?> GetByIdWithIncludesAsync(int id, CancellationToken ct = default);

    Task AddRelationTypeAsync(string name, CancellationToken ct = default);
    Task DeleteRelationTypeAsync(int id, CancellationToken ct = default);
    Task AddEducationLevelAsync(string name, CancellationToken ct = default);
    Task DeleteEducationLevelAsync(int id, CancellationToken ct = default);

    Task<bool> IsRelationTypeInUseAsync(int relationTypeId, CancellationToken ct = default);
    Task<bool> IsEducationLevelInUseAsync(int educationLevelId, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}