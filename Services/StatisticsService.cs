namespace Inspector.Services;
public sealed class StatisticsService : IStatisticsService
{
    private readonly IEmployeesService _employeesService;
    private readonly IPensionAgeSettingsService _pensionSettings;

    public StatisticsService(IEmployeesService employeesService, IPensionAgeSettingsService pensionSettings)
    {
        _employeesService = employeesService;
        _pensionSettings = pensionSettings;
    }

    public async Task<EmployeeStatistics> GetEmployeeStatisticsAsync(CancellationToken ct = default)
    {
        var employees = await _employeesService.GetAllAsync(ct);
        var list = employees.ToList();

        var total = list.Count;
        var male = list.Count(e => e.IsMale);
        var female = total - male;

        var today = DateTime.Today;
        var onVacation = list.Count(emp =>
            emp.VacationRecords != null &&
            emp.VacationRecords.Any(v =>
                v.StartDate.HasValue && v.StartDate.Value.Date <= today &&
                v.EndDate.HasValue && v.EndDate.Value.Date >= today));

        var settings = await _pensionSettings.GetSettingsAsync(ct);
        var menAge = settings?.MenAge ?? 65;
        var womenAge = settings?.WomenAge ?? 60;

        var pensionAge = list.Count(e =>
        {
            var age = GetAge(e.BirthDate);
            return e.IsMale ? age >= menAge : age >= womenAge;
        });

        var avgAge = total > 0
            ? (int)list.Average(e => GetAge(e.BirthDate))
            : 0;

        return new EmployeeStatistics(total, male, female, onVacation, pensionAge, avgAge);
    }

    private static int GetAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }
}
