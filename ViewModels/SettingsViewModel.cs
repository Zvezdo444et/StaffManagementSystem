using Inspector.Models;
using Inspector.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Inspector.ViewModels;

public sealed class SettingsViewModel : ObservableObject
{
    private readonly IEmployeesService _employeesService;
    private readonly IAuthService _authService;
    private readonly User _currentUser;  

    private readonly ObservableCollection<RelationType> _relationTypes = new();
    private readonly ObservableCollection<EducationLevel> _educationLevels = new();

    private string _newRelationTypeName = string.Empty;
    private string _newEducationLevelName = string.Empty;

    private string _oldPassword = "";
    private string _newPassword = "";
    private string _confirmPassword = "";
    private string _passwordChangeMessage = "";
    private Brush _passwordChangeMessageColor = Brushes.Black;

    private readonly DispatcherTimer _messageTimer = new DispatcherTimer
    {
        Interval = TimeSpan.FromSeconds(3)  
    };
    public SettingsViewModel(IEmployeesService employeesService,
                             IAuthService authService,
                             User currentUser)
    {
        _employeesService = employeesService;
        _authService = authService;
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

        LoadCommand = new AsyncRelayCommand(LoadAsync);

        AddRelationTypeCommand = new AsyncRelayCommand(AddRelationTypeAsync);
        DeleteRelationTypeCommand = new RelayCommand<RelationType>(DeleteRelationType);

        AddEducationLevelCommand = new AsyncRelayCommand(AddEducationLevelAsync);
        DeleteEducationLevelCommand = new RelayCommand<EducationLevel>(DeleteEducationLevel);

        ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync);

        _messageTimer.Tick += (s, e) =>
        {
            _messageTimer.Stop();
            PasswordChangeMessage = string.Empty;
        };
    }

    public ObservableCollection<RelationType> RelationTypes => _relationTypes;
    public ObservableCollection<EducationLevel> EducationLevels => _educationLevels;

    public string NewRelationTypeName
    {
        get => _newRelationTypeName;
        set => SetProperty(ref _newRelationTypeName, value);
    }

    public string NewEducationLevelName
    {
        get => _newEducationLevelName;
        set => SetProperty(ref _newEducationLevelName, value);
    }

    public string OldPassword
    {
        get => _oldPassword;
        set => SetProperty(ref _oldPassword, value);
    }

    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public string PasswordChangeMessage
    {
        get => _passwordChangeMessage;
        set => SetProperty(ref _passwordChangeMessage, value);
    }

    public Brush PasswordChangeMessageColor
    {
        get => _passwordChangeMessageColor;
        set => SetProperty(ref _passwordChangeMessageColor, value);
    }

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand AddRelationTypeCommand { get; }
    public ICommand DeleteRelationTypeCommand { get; }
    public AsyncRelayCommand AddEducationLevelCommand { get; }
    public ICommand DeleteEducationLevelCommand { get; }
    public AsyncRelayCommand ChangePasswordCommand { get; }

    private async Task LoadAsync()
    {
        var relations = await _employeesService.GetRelationTypesAsync();
        _relationTypes.Clear();
        foreach (var r in relations) _relationTypes.Add(r);

        var levels = await _employeesService.GetEducationLevelsAsync();
        _educationLevels.Clear();
        foreach (var l in levels) _educationLevels.Add(l);
    }

    private async Task AddRelationTypeAsync()
    {
        if (string.IsNullOrWhiteSpace(NewRelationTypeName)) return;
        await _employeesService.AddRelationTypeAsync(NewRelationTypeName.Trim());
        NewRelationTypeName = string.Empty;
        await LoadAsync();
    }

    private async Task AddEducationLevelAsync()
    {
        if (string.IsNullOrWhiteSpace(NewEducationLevelName)) return;
        await _employeesService.AddEducationLevelAsync(NewEducationLevelName.Trim());
        NewEducationLevelName = string.Empty;
        await LoadAsync();
    }

    private void DeleteRelationType(RelationType? type)
    {
        if (type is null) return;
        DeleteWithCheck(type, "вид родства", type.Name,
            () => _employeesService.IsRelationTypeInUseAsync(type.Id),
            () => _employeesService.DeleteRelationTypeAsync(type.Id));
    }

    private void DeleteEducationLevel(EducationLevel? level)
    {
        if (level is null) return;
        DeleteWithCheck(level, "уровень образования", level.Name,
            () => _employeesService.IsEducationLevelInUseAsync(level.Id),
            () => _employeesService.DeleteEducationLevelAsync(level.Id));
    }

    private void DeleteWithCheck<T>(T item, string entityType, string name,
        Func<Task<bool>> isInUseFunc, Func<Task> deleteFunc)
    {
        Task.Run(async () =>
        {
            try
            {
                bool inUse = await isInUseFunc();
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (inUse)
                    {
                        MessageBox.Show($"Невозможно удалить {entityType} «{name}», так как он используется.",
                            "Удаление невозможно", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var confirm = MessageBox.Show($"Удалить {entityType} «{name}»?",
                        "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (confirm == MessageBoxResult.Yes)
                    {
                        Task.Run(async () =>
                        {
                            await deleteFunc();
                            await Application.Current.Dispatcher.InvokeAsync(LoadAsync);
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error));
            }
        });
    }

    private async Task ChangePasswordAsync()
    {
        PasswordChangeMessage = "";
        PasswordChangeMessageColor = Brushes.Black;

        if (string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            PasswordChangeMessage = "Заполните новый пароль и подтверждение";
            PasswordChangeMessageColor = Brushes.Red;
            _messageTimer.Start();
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            PasswordChangeMessage = "Новые пароли не совпадают";
            PasswordChangeMessageColor = Brushes.Red;
            _messageTimer.Start();
            return;
        }

        if (NewPassword.Length < 4)
        {
            PasswordChangeMessage = "Новый пароль должен быть не менее 4 символов";
            PasswordChangeMessageColor = Brushes.Red;
            _messageTimer.Start();
            return;
        }

        try
        {
            bool success = await _authService.ChangePasswordAsync(
                _currentUser.Id,
                OldPassword ?? "",       
                NewPassword);

            if (success)
            {
                PasswordChangeMessage = "Пароль успешно изменён!";
                PasswordChangeMessageColor = Brushes.Green;
                OldPassword = NewPassword = ConfirmPassword = "";
                _messageTimer.Start();
            }
            else
            {
                PasswordChangeMessage = string.IsNullOrEmpty(_currentUser.PasswordHash)
                    ? "Установите первый пароль"
                    : "Старый пароль введён неверно";
                PasswordChangeMessageColor = Brushes.Red;
                _messageTimer.Start();
            }
        }
        catch (Exception ex)
        {
            PasswordChangeMessage = $"Ошибка: {ex.Message}";
            PasswordChangeMessageColor = Brushes.Red;
            _messageTimer.Start();
        }
    }
}