using Inspector.Models;
using Inspector.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace Inspector.ViewModels.Employees;

public sealed class EmployeeSelectorViewModel : ValidatableViewModelBase
{
    private readonly IEmployeesService _service;
    private string _searchText = string.Empty;
    private bool _sortAscending = true;

    public EmployeeSelectorViewModel(IEmployeesService service)
    {
        _service = service;

        EmployeesView = CollectionViewSource.GetDefaultView(Employees);
        EmployeesView.Filter = Filter;

        SelectEmployeeCommand = new RelayCommand<Employee>(OnSelect);
        ToggleSortCommand = new RelayCommand(ToggleSort);

        SelectedSortOption = SortOptions.FirstOrDefault() ?? "По фамилии (А-Я)";

        LoadAsync();
    }

    public ObservableCollection<Employee> Employees { get; } = new();
    public ICollectionView EmployeesView { get; }

    public string Subtitle { get; set; } = "Выберите работника для просмотра персональных данных";

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                EmployeesView.Refresh();
        }
    }

    public string CurrentSortText => _sortAscending ? "названию ↑" : "названию ↓";

    public ICommand SelectEmployeeCommand { get; set; } = null!;
    public RelayCommand ToggleSortCommand { get; }

    private async void LoadAsync()
    {
        var list = await _service.GetAllAsync();
        Employees.Clear();
        foreach (var emp in list)
            Employees.Add(emp);

        EmployeesView.Refresh();
    }

    private bool Filter(object obj)
    {
        if (obj is not Employee e) return false;
        return string.IsNullOrWhiteSpace(SearchText) ||
               e.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    private void ToggleSort()
    {
        _sortAscending = !_sortAscending;
        EmployeesView.SortDescriptions.Clear();
        EmployeesView.SortDescriptions.Add(new SortDescription(
            nameof(Employee.FullName),
            _sortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));

        OnPropertyChanged(nameof(CurrentSortText));
    }

    private void OnSelect(Employee? employee)
    {
    }

    public List<string> SortOptions { get; } = new() { "По фамилии (А-Я)", "По фамилии (Я-А)" };

    private string _selectedSortOption = "По фамилии (А-Я)";
    public string SelectedSortOption
    {
        get => _selectedSortOption;
        set
        {
            if (SetProperty(ref _selectedSortOption, value))
                ApplySort();
        }
    }

    private void ApplySort()
    {
        EmployeesView.SortDescriptions.Clear();
        var direction = _selectedSortOption.Contains("Я-А")
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;

        EmployeesView.SortDescriptions.Add(new SortDescription("FullName", direction));
    }

}