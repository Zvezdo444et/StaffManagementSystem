using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Inspector.ViewModels;

namespace Inspector.Views;

public partial class PensionAgeView : UserControl
{
    public PensionAgeView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => TriggerLoad();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TriggerLoad();
    }

    private void TriggerLoad()
    {
        if (DataContext is not PensionAgeViewModel vm)
            return;
        if (vm.IsLoading)
            return;

        Dispatcher.BeginInvoke(() => vm.LoadCommand.Execute(null), DispatcherPriority.Background);
    }
}
