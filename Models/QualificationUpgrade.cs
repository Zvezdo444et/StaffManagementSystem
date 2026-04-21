using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Inspector.Models;

public sealed class QualificationUpgrade : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public QualificationUpgrade() { }

    private int _id;
    private int _employeeId;
    private DateTime? _startDate;
    private int? _daysCount;
    private DateTime? _certificateDate;
    private string _upgradeType = string.Empty;
    private string? _certificateNumber;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public int EmployeeId
    {
        get => _employeeId;
        set => SetProperty(ref _employeeId, value);
    }

    public Employee Employee { get; set; } = null!;

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

    [Column("Days")]
    public int? DaysCount
    {
        get => _daysCount;
        set
        {
            if (SetProperty(ref _daysCount, value))
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
            if (StartDate.HasValue && DaysCount.HasValue)
                return StartDate.Value.AddDays(DaysCount.Value - 1);
            return null;
        }
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

    public string UpgradeType
    {
        get => _upgradeType;
        set => SetProperty(ref _upgradeType, value);
    }

    public DateTime? CertificateDate
    {
        get => _certificateDate;
        set => SetProperty(ref _certificateDate, value);
    }

    public string? CertificateNumber
    {
        get => _certificateNumber;
        set => SetProperty(ref _certificateNumber, value);
    }

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

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
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
