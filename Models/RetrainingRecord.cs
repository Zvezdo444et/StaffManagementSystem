using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Inspector.Models;

public sealed class RetrainingRecord : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private DateTime? _startDate;
    private int? _daysCount;
    private string _specialty = string.Empty;
    private DateTime? _diplomaDate;
    private string? _diplomaNumber;

    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public DateTime? StartDate
    {
        get => _startDate;
        set { if (SetProp(ref _startDate, value)) { OnPropertyChanged(nameof(EndDate)); OnPropertyChanged(nameof(DaysCountText)); } }
    }

    [Column("Days")]
    public int? DaysCount
    {
        get => _daysCount;
        set { if (SetProp(ref _daysCount, value)) { OnPropertyChanged(nameof(EndDate)); OnPropertyChanged(nameof(DaysCountText)); } }
    }

    [NotMapped]
    public DateTime? EndDate
    {
        get => StartDate.HasValue && DaysCount.HasValue ? StartDate.Value.AddDays(DaysCount.Value - 1) : null;
        set
        {
            if (value.HasValue && StartDate.HasValue)
            {
                var days = (value.Value - StartDate.Value).Days + 1;
                if (days > 0)
                    DaysCount = days;
                else
                    DaysCount = null;
            }
            else
            {
                DaysCount = null;
            }
            
            OnPropertyChanged(nameof(EndDate));
            OnPropertyChanged(nameof(DaysCountText));
        }
    }

    public string Specialty { get => _specialty; set => SetProp(ref _specialty, value); }
    public DateTime? DiplomaDate { get => _diplomaDate; set => SetProp(ref _diplomaDate, value); }
    public string? DiplomaNumber { get => _diplomaNumber; set => SetProp(ref _diplomaNumber, value); }

    [NotMapped]
    public string? Period
    {
        get
        {
            if (StartDate.HasValue)
            {
                if (DaysCount.HasValue)
                {
                    var end = StartDate.Value.AddDays(DaysCount.Value - 1);
                    return $"{StartDate.Value:dd.MM.yyyy} - {end:dd.MM.yyyy}";
                }
                return $"{StartDate.Value:dd.MM.yyyy} - ...";
            }
            return null;
        }
    }

    [NotMapped]
    public string DaysCountText
    {
        get => DaysCount?.ToString() ?? "";
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                DaysCount = null;
            else if (int.TryParse(value, out var days))
                DaysCount = days;
        }
    }

    private bool SetProp<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}