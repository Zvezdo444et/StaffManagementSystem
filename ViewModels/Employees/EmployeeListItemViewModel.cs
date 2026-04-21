using Inspector.Models;

namespace Inspector.ViewModels.Employees;

public sealed class EmployeeListItemViewModel : ObservableObject
{
    public EmployeeListItemViewModel(Employee employee)
    {
        Id = employee.Id;
        FirstName = employee.FirstName;
        MiddleName = employee.MiddleName ?? string.Empty;
        FullName = employee.GetFullName();
        Position = employee.CurrentProfession ?? string.Empty;   
        BirthDate = employee.BirthDate;
        Phone = employee.Phone ?? string.Empty;
    }

    public int Id { get; }
    public string FullName { get; }
    public string FirstName { get; }     
    public string MiddleName { get; }
    public string Position { get; }       
    public DateTime BirthDate { get; }

    public int Age
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - BirthDate.Year;
            if (BirthDate.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

    public string Phone { get; }

    public string BirthDateAndAge => $"{BirthDate:dd.MM.yyyy} / {Age} лет";
}