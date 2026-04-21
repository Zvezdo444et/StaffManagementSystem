using Inspector.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Inspector.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.LoadCommand.Execute(null);
            }
        }
    }
}