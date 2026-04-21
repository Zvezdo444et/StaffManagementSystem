using System.ComponentModel.DataAnnotations.Schema;

namespace Inspector.Models;

public sealed class Employee
{
    public int Id { get; set; }
    public string FullName => $"{LastName} {FirstName} {MiddleName ?? string.Empty}".Trim();
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public bool IsMale { get; set; }
    public DateTime BirthDate { get; set; }
    public string? BirthPlace { get; set; }
    public bool IsUnionMember { get; set; }
    public int? SpecialtyExperienceYears { get; set; }
    public int? TotalExperienceYears { get; set; }
    public string? Category { get; set; }       
    public string? Address { get; set; }
    public string? DiplomaSpecialty { get; set; }
    public string? DiplomaQualification { get; set; }
    public string? MainProfession { get; set; }
    public string? CurrentProfession { get; set; }
    public string? Phone { get; set; }
    public string? HomePhone { get; set; }
    public DateTime? HireDate { get; set; }    
    public string? LastWorkPlace { get; set; }
    public string? LastWorkPosition { get; set; }
    public DateTime? LastWorkDismissalDate { get; set; }
    public string? LastWorkDismissalReason { get; set; }
    public string? AdditionalInfo { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public MilitaryRegistration? MilitaryRegistration { get; set; }
    public Passport? Passport { get; set; }

    public ICollection<VacationRecord> VacationRecords { get; set; } = new List<VacationRecord>();
    public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
    public ICollection<EducationRecord> EducationRecords { get; set; } = new List<EducationRecord>();
    public ICollection<AppointmentTransfer> AppointmentTransfers { get; set; } = new List<AppointmentTransfer>();
    public ICollection<QualificationUpgrade> QualificationUpgrades { get; set; } = new List<QualificationUpgrade>();
    public ICollection<RetrainingRecord> RetrainingRecords { get; set; } = new List<RetrainingRecord>();
    public ICollection<AttestationRecord> AttestationRecords { get; set; } = new List<AttestationRecord>();

    public string GetFullName()
    {
        return string.Join(" ", new[] { LastName, FirstName, MiddleName }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}