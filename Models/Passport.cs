namespace Inspector.Models;

public sealed class Passport
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public string? Series { get; set; }
    public string? Number { get; set; }
    public string? IdentificationNumber { get; set; }
    public string? IssuedBy { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

}

