using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Inspector.Models;

public sealed class AttestationRecord : INotifyPropertyChanged
{
    private DateTime? _date;
    private string _commissionDecision = string.Empty;

    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public DateTime? Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
    }

    public string CommissionDecision
    {
        get => _commissionDecision;
        set => SetProperty(ref _commissionDecision, value);
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