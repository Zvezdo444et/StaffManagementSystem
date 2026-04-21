using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Inspector.Models;

public sealed class AppointmentTransfer : INotifyPropertyChanged
{
    private DateTime? _date;
    private string? _position;
    private string? _basisDocumentName;
    private DateTime? _documentDate;         
    private string? _basisDocumentNumber;
    private string? _contractType;
    private string? _contractTerm;

    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    [Required(ErrorMessage = "Дата назначения обязательна")]
    public DateTime? Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
    }

    [Required(ErrorMessage = "Должность обязательна")]
    public string? Position
    {
        get => _position;
        set => SetProperty(ref _position, value);
    }

    public string? BasisDocumentName
    {
        get => _basisDocumentName;
        set => SetProperty(ref _basisDocumentName, value);
    }

    public DateTime? DocumentDate  
    {
        get => _documentDate;
        set => SetProperty(ref _documentDate, value);
    }

    public string? BasisDocumentNumber
    {
        get => _basisDocumentNumber;
        set => SetProperty(ref _basisDocumentNumber, value);
    }

    public string? ContractType
    {
        get => _contractType;
        set => SetProperty(ref _contractType, value);
    }

    public string? ContractTerm
    {
        get => _contractTerm;
        set => SetProperty(ref _contractTerm, value);
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