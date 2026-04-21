using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Inspector.ViewModels.Employees;

namespace Inspector.Views.Employees;

public partial class EmployeesListView : UserControl
{
    public EmployeesListView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not EmployeesListViewModel vm)
            return;
        if (vm.IsLoading || vm.Employees.Count > 0)
            return;
        Dispatcher.BeginInvoke(() => vm.LoadCommand.Execute(null), DispatcherPriority.Background);
    }
}

