namespace Inspector.Models;

public sealed class MilitaryRegistration
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public string? AccountingGroup { get; set; }
    public string? AccountingCategory { get; set; }
    public string? Composition { get; set; }
    public string? MilitaryRank { get; set; }
    public string? MilitarySpecialty { get; set; }
    public string? FitnessCategory { get; set; }
    public string? CommissariatName { get; set; }
    public string? SpecialAccountingNumber { get; set; }
}

