using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Inspector.Views.Behaviors;
public static class DataGridShiftWheelScrollBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(DataGridShiftWheelScrollBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid)
            return;

        if ((bool)e.NewValue)
            grid.PreviewMouseWheel += OnPreviewMouseWheel;
        else
            grid.PreviewMouseWheel -= OnPreviewMouseWheel;
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not DataGrid grid)
            return;
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            return;

        var scrollViewer = FindScrollViewer(grid);
        if (scrollViewer == null)
            return;

        double delta = -e.Delta * 0.3; 
        double offset = scrollViewer.HorizontalOffset + delta;
        offset = System.Math.Max(0, System.Math.Min(offset, scrollViewer.ScrollableWidth));
        scrollViewer.ScrollToHorizontalOffset(offset);
        e.Handled = true;
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject root)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is ScrollViewer sv)
                return sv;
            var found = FindScrollViewer(child);
            if (found != null)
                return found;
        }
        return null;
    }
}
