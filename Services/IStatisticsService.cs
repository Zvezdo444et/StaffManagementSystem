namespace Inspector.Services;
public interface IStatisticsService
{
    Task<EmployeeStatistics> GetEmployeeStatisticsAsync(CancellationToken ct = default);
}
public sealed record EmployeeStatistics(
    int TotalEmployees,
    int MaleEmployees,
    int FemaleEmployees,
    int OnVacationEmployees,
    int PensionAgeEmployees,
    int AverageAge);
