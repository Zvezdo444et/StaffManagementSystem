using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inspector.Models;

[Table("VacationRecord")]
public class VacationRecord : INotifyPropertyChanged
{
    private string? _basis;
    private int? _workingDays;
    private DateTime? _startDate;
    private string? _vacationKind;

    public int Id { get; set; }
    public int EmployeeId { get; set; }

    public string VacationKind
    {
        get => _vacationKind;
        set => SetProperty(ref _vacationKind, value);
    }

    public string? Basis
    {
        get => _basis;
        set => SetProperty(ref _basis, value);
    }

    public int? WorkingDays
    {
        get => _workingDays;
        set
        {
            if (SetProperty(ref _workingDays, value))
            {
                OnPropertyChanged(nameof(EndDate));
                OnPropertyChanged(nameof(DaysCountText));
            }
        }
    }

    public DateTime? StartDate
    {
        get => _startDate;
        set
        {
            if (SetProperty(ref _startDate, value))
            {
                OnPropertyChanged(nameof(EndDate));
                OnPropertyChanged(nameof(DaysCountText));
            }
        }
    }

    [NotMapped]
    public DateTime? EndDate
    {
        get
        {
            if (StartDate.HasValue && WorkingDays.HasValue)
                return StartDate.Value.AddDays(WorkingDays.Value - 1);
            return null;
        }
        set
        {
            if (value.HasValue && StartDate.HasValue)
            {
                var days = (value.Value - StartDate.Value).Days + 1;
                WorkingDays = days > 0 ? days : null;
            }
            else
            {
                WorkingDays = null;
            }

            OnPropertyChanged(nameof(EndDate));
            OnPropertyChanged(nameof(DaysCountText));
        }
    }

    [NotMapped]
    public string? Period
    {
        get
        {
            if (StartDate.HasValue)
            {
                if (WorkingDays.HasValue)
                {
                    var end = StartDate.Value.AddDays(WorkingDays.Value - 1);
                    return $"{StartDate.Value:dd.MM.yyyy} - {end:dd.MM.yyyy}";
                }
                return $"{StartDate.Value:dd.MM.yyyy} - ...";
            }
            return null;
        }
    }

    [NotMapped]
    public string? DaysCountText
    {
        get => WorkingDays?.ToString() ?? "";
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                WorkingDays = null;
            else if (int.TryParse(value, out var days))
                WorkingDays = days;
        }
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