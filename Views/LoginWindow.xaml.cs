using Inspector.Models;
using Inspector.Services;
using Inspector.ViewModels;
using System.Windows;

namespace Inspector.Views;

public partial class LoginWindow : Window
{
    public LoginWindow(IAuthService authService, Action<User> onSuccessfulLogin)
    {
        InitializeComponent();
        DataContext = new LoginViewModel(authService, onSuccessfulLogin);
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.Password = PasswordBox.Password;
        }
    }
}