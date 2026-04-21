using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Inspector.Models;

public sealed class FamilyMember : INotifyPropertyChanged
{
    private string _lastName = string.Empty;
    private string _firstName = string.Empty;
    private string? _middleName;
    private DateTime? _birthDate;
    private int? _relationTypeId;
    private RelationType? _relationType;

    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public string LastName
    {
        get => _lastName;
        set { if (SetProperty(ref _lastName, value)) OnPropertyChanged(nameof(FullName)); }
    }

    public string FirstName
    {
        get => _firstName;
        set { if (SetProperty(ref _firstName, value)) OnPropertyChanged(nameof(FullName)); }
    }

    public string? MiddleName
    {
        get => _middleName;
        set { if (SetProperty(ref _middleName, value)) OnPropertyChanged(nameof(FullName)); }
    }

    public DateTime? BirthDate
    {
        get => _birthDate;
        set { if (SetProperty(ref _birthDate, value)) OnPropertyChanged(nameof(FullName)); }
    }

    public int? RelationTypeId
    {
        get => _relationTypeId;
        set => SetProperty(ref _relationTypeId, value);
    }

    public RelationType? RelationType
    {
        get => _relationType;
        set
        {
            if (SetProperty(ref _relationType, value))
            {
                RelationTypeId = value?.Id;
                OnPropertyChanged(nameof(RelationTypeStr));
                OnPropertyChanged(nameof(CustomRelationType));
            }
        }
    }

    public string CustomRelationType
    {
        get => RelationType?.Name ?? string.Empty;
        set
        {
            if (RelationType?.Name == value) return;
            if (string.IsNullOrWhiteSpace(value))
            {
                RelationType = null;
            }
            else
            {
                RelationType = new RelationType { Name = value.Trim() };
            }
            OnPropertyChanged(nameof(CustomRelationType));
        }
    }

    public string RelationTypeStr => RelationType?.Name ?? string.Empty;

    public string FullName =>
        string.Join(" ", new[] { LastName, FirstName, MiddleName }.Where(x => !string.IsNullOrWhiteSpace(x)));

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