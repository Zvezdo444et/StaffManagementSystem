using Inspector.ViewModels.Employees;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

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

    private void DataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not EmployeesListViewModel vm)
            return;

        var element = e.OriginalSource as FrameworkElement;
        var row = FindVisualParent<DataGridRow>(element);
        if (row == null)
            return;

        if (vm.SelectedEmployee != null && vm.OpenEmployeeCommand.CanExecute(vm.SelectedEmployee))
            vm.OpenEmployeeCommand.Execute(vm.SelectedEmployee);
    }

    private static T? FindVisualParent<T>(DependencyObject? child) where T : DependencyObject
    {
        while (child != null)
        {
            if (child is T target)
                return target;
            child = VisualTreeHelper.GetParent(child);
        }
        return null;
    }
}

