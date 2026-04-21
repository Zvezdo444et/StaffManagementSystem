using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Inspector.Models;

public sealed class EducationRecord : INotifyPropertyChanged
{
    private EducationLevel? _educationLevel;
    private string _customEducationLevel = string.Empty;
    private string? _institutionName;
    private DateTime? _graduationDate;
    private string? _studyType;
    private string? _specialty;
    private string? _qualification;

    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public EducationLevel? EducationLevel
    {
        get => _educationLevel;
        set
        {
            if (SetProperty(ref _educationLevel, value))
                OnPropertyChanged(nameof(CustomEducationLevel));
        }
    }

    public string CustomEducationLevel
    {
        get => EducationLevel?.Name ?? _customEducationLevel;
        set
        {
            if (EducationLevel?.Name == value) return;
            if (string.IsNullOrWhiteSpace(value))
            {
                EducationLevel = null;
                _customEducationLevel = string.Empty;
            }
            else
            {
                EducationLevel = new EducationLevel { Name = value.Trim() };
                _customEducationLevel = value.Trim();
            }
            OnPropertyChanged(nameof(CustomEducationLevel));
        }
    }

    public string? InstitutionName
    {
        get => _institutionName;
        set => SetProperty(ref _institutionName, value);
    }

    public DateTime? GraduationDate
    {
        get => _graduationDate;
        set => SetProperty(ref _graduationDate, value);
    }

    public string? StudyType
    {
        get => _studyType;
        set => SetProperty(ref _studyType, value);
    }

    public string? Specialty
    {
        get => _specialty;
        set => SetProperty(ref _specialty, value);
    }

    public string? Qualification
    {
        get => _qualification;
        set => SetProperty(ref _qualification, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value)) return false;
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}