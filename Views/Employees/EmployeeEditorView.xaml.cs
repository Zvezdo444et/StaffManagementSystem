using System.Windows.Controls;
using System.Windows.Input;

namespace Inspector.Views.Employees;

public partial class EmployeeEditorView : UserControl
{
    public EmployeeEditorView()
    {
        InitializeComponent();
    }

    private void TabsHeaderScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer sv)
            return;

        if (e.Delta == 0)
            return;

        sv.ScrollToHorizontalOffset(sv.HorizontalOffset - e.Delta);
        e.Handled = true;
    }
}

