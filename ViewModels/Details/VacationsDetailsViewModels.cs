using Inspector.Models;
using System.Windows.Input;

namespace Inspector.ViewModels.Details;

public sealed class VacationsDetailsViewModel : ObservableObject
{
    private readonly Employee _employee;
    private readonly Action _onClose;
    private readonly Action<Employee> _onEdit;

    public Employee Employee => _employee;

    public ICommand EditCommand { get; }
    public ICommand CloseCommand { get; }

    public VacationsDetailsViewModel(
        Employee employee,
        Action onClose,
        Action<Employee> onEdit)
    {
        _employee = employee;
        _onClose = onClose;
        _onEdit = onEdit;

        EditCommand = new RelayCommand(() => _onEdit?.Invoke(_employee));
        CloseCommand = new RelayCommand(() => _onClose?.Invoke());
    }
}