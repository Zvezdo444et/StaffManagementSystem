using Inspector.Models;
using Inspector.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Inspector.ViewModels.Employees;

public sealed class EmployeesListViewModel : ValidatableViewModelBase
{
    private readonly IEmployeesService _employeesService;

    private string _searchText = string.Empty;
    private SortOption _selectedSortOption;
    private bool _isLoading;
    private EmployeeListItemViewModel? _selectedEmployee;
    private string _errorMessage = string.Empty;
    private bool _isEditorOpen;
    private EmployeeEditViewModel? _editorViewModel;

    public EmployeesListViewModel(IEmployeesService employeesService)
    {
        _employeesService = employeesService;

        SortOptions = new ReadOnlyCollection<SortOption>(new[]
        {
            new SortOption("Сортировать по: (А→Я)", nameof(EmployeeListItemViewModel.FullName), ListSortDirection.Ascending),
            new SortOption("Сортировать по: названию (Я→А)", nameof(EmployeeListItemViewModel.FullName), ListSortDirection.Descending)
        });

        _selectedSortOption = SortOptions[0];

        Employees = new ObservableCollection<EmployeeListItemViewModel>();
        EmployeesView = CollectionViewSource.GetDefaultView(Employees);
        EmployeesView.Filter = FilterEmployee;
        ApplySorting();

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        AddEmployeeCommand = new RelayCommand(OpenAddEmployee);
        OpenEmployeeCommand = new RelayCommand<EmployeeListItemViewModel>(OpenEmployeeAsync);
        DeleteEmployeeCommand = new RelayCommand<EmployeeListItemViewModel>(DeleteEmployeeAsync);
    }

    private async void DeleteEmployeeAsync(EmployeeListItemViewModel? item)
    {
        if (item == null) return;
        string fullName = item.FullName;
        string nameAndPatronymic = $"{item.FirstName} {item.MiddleName}".Trim();

        string input = Microsoft.VisualBasic.Interaction.InputBox(
            $"Для подтверждения удаления введите *ИМЯ и ОТЧЕСТВО* работника:\n\n{fullName}",
            "Подтверждение удаления",
            "");

        if (string.IsNullOrWhiteSpace(input)) return;

        string Normalize(string s) => s.ToLowerInvariant()
                                       .Replace(" ", "")
                                       .Replace(",", "")
                                       .Replace(".", "")
                                       .Replace("ё", "е")
                                       .Replace("Ё", "е");

        if (Normalize(input) != Normalize(nameAndPatronymic))
        {
            MessageBox.Show("Введённые данные не совпадают с именем и отчеством работника.\nУдаление отменено.",
                            "Ошибка подтверждения", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var confirm = MessageBox.Show($"Вы действительно хотите НАВЕСГДА удалить работника?\n\n{item.FullName}",
                                      "ВНИМАНИЕ! Необратимое действие",
                                      MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        await _employeesService.DeleteAsync(item.Id);
        await LoadAsync();
    }

    private async void OpenEmployeeAsync(EmployeeListItemViewModel? item)
    {
        if (item == null) return;

        var fullEmployee = await _employeesService.GetByIdWithIncludesAsync(item.Id);
        if (fullEmployee == null)
        {
            MessageBox.Show("Не удалось загрузить данные сотрудника", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        EditorViewModel = new EmployeeEditViewModel(
            _employeesService,
            OnEmployeeSaved,
            OnEditorClosed,
            fullEmployee);

        IsEditorOpen = true;
    }

    private void OnEmployeeSaved(Employee savedEmployee)
    {
        IsEditorOpen = false;
        LoadCommand.Execute(null);
    }

    private void OnEditorClosed()
    {
        IsEditorOpen = false;
        EditorViewModel = null;
    }

    public ObservableCollection<EmployeeListItemViewModel> Employees { get; }
    public ICollectionView EmployeesView { get; }
    public IReadOnlyList<SortOption> SortOptions { get; }

    public SortOption SelectedSortOption
    {
        get => _selectedSortOption;
        set
        {
            if (!SetProperty(ref _selectedSortOption, value)) return;
            ApplySorting();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (!SetProperty(ref _searchText, value)) return;
            ValidateSearchText();
            EmployeesView.Refresh();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
                OnPropertyChanged(nameof(HasErrorMessage));
        }
    }

    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

    public AsyncRelayCommand LoadCommand { get; }
    public RelayCommand AddEmployeeCommand { get; }
    public RelayCommand<EmployeeListItemViewModel> DeleteEmployeeCommand { get; }
    public RelayCommand<EmployeeListItemViewModel> OpenEmployeeCommand { get; }

    public EmployeeListItemViewModel? SelectedEmployee
    {
        get => _selectedEmployee;
        set => SetProperty(ref _selectedEmployee, value);
    }

    public bool IsEditorOpen
    {
        get => _isEditorOpen;
        set => SetProperty(ref _isEditorOpen, value);
    }

    public EmployeeEditViewModel? EditorViewModel
    {
        get => _editorViewModel;
        private set => SetProperty(ref _editorViewModel, value);
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            Employees.Clear();
            var employees = await _employeesService.GetAllAsync();
            foreach (var e in employees)
            {
                Employees.Add(new EmployeeListItemViewModel(e));
            }
            EmployeesView.Refresh();

            if (Employees.Count == 0)
                ErrorMessage = "В базе данных нет активных сотрудников.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки списка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OpenAddEmployee()
    {
        EditorViewModel = new EmployeeEditViewModel(
            _employeesService,
            onSaved: _ => { IsEditorOpen = false; LoadCommand.Execute(null); },
            onCancelled: () => IsEditorOpen = false);

        IsEditorOpen = true;
    }

    private bool FilterEmployee(object obj)
    {
        if (obj is not EmployeeListItemViewModel item)
            return false;

        var q = (SearchText ?? string.Empty).Trim();
        if (q.Length == 0) return true;

        return item.FullName.Contains(q, StringComparison.CurrentCultureIgnoreCase) ||
               item.Position.Contains(q, StringComparison.CurrentCultureIgnoreCase) ||
               item.Phone.Contains(q, StringComparison.CurrentCultureIgnoreCase);
    }

    private void ApplySorting()
    {
        EmployeesView.SortDescriptions.Clear();
        EmployeesView.SortDescriptions.Add(new SortDescription(
            SelectedSortOption.PropertyName,
            SelectedSortOption.Direction));
    }

    private void ValidateSearchText()
    {
        ClearErrors(nameof(SearchText));
        if (SearchText.Length > 100)
            AddError(nameof(SearchText), "Строка поиска слишком длинная (макс. 100 символов).");
    }

    public sealed record SortOption(string Title, string PropertyName, ListSortDirection Direction)
    {
        public override string ToString() => Title;
    }
}