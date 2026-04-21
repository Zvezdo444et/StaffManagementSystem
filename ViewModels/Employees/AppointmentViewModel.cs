using Inspector.Models;
using Inspector.Services;
using Inspector.ViewModels.Details;
using Inspector.ViewModels.Employees;
using System.Windows.Input;

namespace Inspector.ViewModels.Employees;

public sealed class AppointmentViewModel : ObservableObject
{
    private readonly IEmployeesService _service;

    public EmployeeSelectorViewModel Selector { get; }

    private bool _isDetailsOpen;
    public bool IsDetailsOpen
    {
        get => _isDetailsOpen;
        set => SetProperty(ref _isDetailsOpen, value);
    }

    private AppointmentsDetailsViewModel? _detailsViewModel;
    public AppointmentsDetailsViewModel? DetailsViewModel
    {
        get => _detailsViewModel;
        private set => SetProperty(ref _detailsViewModel, value);
    }

    private bool _isEditorOpen;
    public bool IsEditorOpen
    {
        get => _isEditorOpen;
        set => SetProperty(ref _isEditorOpen, value);
    }

    private EmployeeEditViewModel? _editorViewModel;
    public EmployeeEditViewModel? EditorViewModel
    {
        get => _editorViewModel;
        private set => SetProperty(ref _editorViewModel, value);
    }

    public AppointmentViewModel(IEmployeesService service)
    {
        _service = service;

        Selector = new EmployeeSelectorViewModel(service)
        {
            Subtitle = "Выберите работника для просмотра назначений и перемещений"
        };

        Selector.SelectEmployeeCommand = new RelayCommand<Employee>(OpenAppointmentData);
    }

    private async void OpenAppointmentData(Employee? employee)
    {
        if (employee == null) return;

        var fullEmployee = await _service.GetByIdWithIncludesAsync(employee.Id);
        if (fullEmployee == null)
        {
            System.Windows.MessageBox.Show("Не удалось загрузить данные сотрудника", "Ошибка",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return;
        }

        DetailsViewModel = new AppointmentsDetailsViewModel(
            fullEmployee,
            () => IsDetailsOpen = false,
            OpenEditor
        );

        IsDetailsOpen = true;
    }

    private void OpenEditor(Employee employee)
    {
        EditorViewModel = new EmployeeEditViewModel(
            _service,
            OnEmployeeSaved,
            OnEditorClosed,
            employee);

        IsEditorOpen = true;
        IsDetailsOpen = false;
    }

    private async void OnEmployeeSaved(Employee savedEmployee)
    {
        IsEditorOpen = false;
        EditorViewModel = null;

        var fullEmployee = await _service.GetByIdWithIncludesAsync(savedEmployee.Id);
        if (fullEmployee == null)
        {
            System.Windows.MessageBox.Show("Не удалось загрузить обновлённые данные", "Ошибка",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return;
        }

        DetailsViewModel = new AppointmentsDetailsViewModel(
            fullEmployee,
            () => IsDetailsOpen = false,
            OpenEditor
        );

        IsDetailsOpen = true;
    }

    private void OnEditorClosed()
    {
        IsEditorOpen = false;
        EditorViewModel = null;
        IsDetailsOpen = true;
    }
}