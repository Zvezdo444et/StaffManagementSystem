using Inspector.Models;
using Inspector.Services;
using System.Windows.Input;

namespace Inspector.ViewModels;

public sealed class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly Action<User> _onSuccessfulLogin;

    private string _login = "";
    private string _password = "";
    private string _errorMessage = "";
    private bool _isLoading;

    public LoginViewModel(IAuthService authService, Action<User> onSuccessfulLogin)
    {
        _authService = authService;
        _onSuccessfulLogin = onSuccessfulLogin;

        LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
    }

    public string Login
    {
        get => _login;
        set
        {
            _login = value?.Trim() ?? "";
            OnPropertyChanged();
            ((AsyncRelayCommand)LoginCommand).RaiseCanExecuteChanged();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value ?? "";
            OnPropertyChanged();
            ((AsyncRelayCommand)LoginCommand).RaiseCanExecuteChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            SetProperty(ref _isLoading, value);
            ((AsyncRelayCommand)LoginCommand).RaiseCanExecuteChanged();
        }
    }

    public ICommand LoginCommand { get; }

    private bool CanLogin() => !IsLoading && !string.IsNullOrWhiteSpace(Login);

    private async Task LoginAsync()
    {
        ErrorMessage = "";
        IsLoading = true;

        try
        {
            var user = await _authService.AuthenticateAsync(Login, Password);

            if (user != null)
            {
                _onSuccessfulLogin(user);
            }
            else
            {
                ErrorMessage = "Неверный логин или пароль";
            }
        }
        catch
        {
            ErrorMessage = "Ошибка соединения с базой данных";
        }
        finally
        {
            IsLoading = false;
        }
    }
}