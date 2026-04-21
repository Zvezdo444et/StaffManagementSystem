using Inspector.Models;
using Inspector.Services;
using Inspector.ViewModels.Employees;
using Inspector.ViewModels.Navigation;
using System.Collections.ObjectModel;
using System.Windows;

namespace Inspector.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly NotificationsViewModel _notificationsViewModel;
    private readonly NavigationStore _navigationStore;
    private readonly Func<EmployeesListViewModel> _employeesListVmFactory;
    private readonly Func<ReportsViewModel> _reportsViewModelFactory;
    private readonly Func<Search.SearchViewModel> _searchViewModelFactory;
    private readonly IEmployeesService _employeesService;
    private readonly IStatisticsService _statistics;
    private readonly IPensionAgeSettingsService _pensionSettings;
    private readonly IEmployeeSearchService _searchService;
    private readonly IAuthService _authService;

    private EmployeesListViewModel? _employeesListViewModel;
    private PensionAgeViewModel? _pensionAgeViewModel;
    private SettingsViewModel? _settingsViewModel;
    private User? _currentUser;

    public User? CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    public MainViewModel(
        NavigationStore navigationStore,
        Func<EmployeesListViewModel> employeesListVmFactory,
        Func<ReportsViewModel> reportsViewModelFactory,
        Func<Search.SearchViewModel> searchViewModelFactory,
        IStatisticsService statistics,
        IPensionAgeSettingsService pensionSettings,
        IEmployeeSearchService searchService,
        IEmployeesService employeesService,
        IAuthService authService,
        NotificationsViewModel notificationsViewModel)
    {
        _navigationStore = navigationStore;
        _employeesListVmFactory = employeesListVmFactory;
        _reportsViewModelFactory = reportsViewModelFactory;
        _searchViewModelFactory = searchViewModelFactory;
        _statistics = statistics;
        _pensionSettings = pensionSettings;
        _searchService = searchService;
        _employeesService = employeesService;
        _authService = authService;
        _notificationsViewModel = notificationsViewModel;

        _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

        var employeesCmd = new RelayCommand(() =>
        {
            _employeesListViewModel ??= _employeesListVmFactory();
            _navigationStore.CurrentViewModel = _employeesListViewModel;
            SetActive("Список работников");
        });

        var searchCmd = new RelayCommand(() =>
        {
            _navigationStore.CurrentViewModel = _searchViewModelFactory();
            SetActive("Поиск");
        });

        var reportsCmd = new RelayCommand(() =>
        {
            _navigationStore.CurrentViewModel = _reportsViewModelFactory();
            SetActive("Отчёты");
        });

        var personalCmd = new RelayCommand(() =>
        {
            _navigationStore.CurrentViewModel = new PersonalDataViewModel(_employeesService);
            SetActive("Персональные данные");
        });

        var qualificationCmd = new RelayCommand(() =>
        {
            _navigationStore.CurrentViewModel = new QualificationViewModel(_employeesService);
            SetActive("Профподготовка");
        });

        var appointmentsCmd = new RelayCommand(() =>
        {
            _navigationStore.CurrentViewModel = new AppointmentViewModel(_employeesService);
            SetActive("Назначения");
        });

        var vacationCmd = new RelayCommand(() =>
        {
            _navigationStore.CurrentViewModel = new VacationViewModel(_employeesService);
            SetActive("Отпуска");
        });

        var pensionCmd = new RelayCommand(() =>
        {
            _pensionAgeViewModel ??= new PensionAgeViewModel(_employeesService, _pensionSettings);
            _navigationStore.CurrentViewModel = _pensionAgeViewModel;
            SetActive("Пенсионный возраст");
            if (!_pensionAgeViewModel.IsLoading)
                _pensionAgeViewModel.LoadCommand.Execute(null);
        });

        var settingsCmd = new RelayCommand(() =>
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Сначала выполните вход в систему.", "Доступ запрещён",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _settingsViewModel ??= new SettingsViewModel(_employeesService, _authService, _currentUser);
            _navigationStore.CurrentViewModel = _settingsViewModel;
            SetActive("Настройки");
            _settingsViewModel.LoadCommand.Execute(null);
        });

        var notificationCmd = new RelayCommand(() =>
        {
            _navigationStore.CurrentViewModel = _notificationsViewModel;
            SetActive("Уведомления");
        });

        NavigationItems = new ObservableCollection<NavigationItemViewModel>
        {
            new("Список работников", employeesCmd),
            new("Поиск", searchCmd),
            new("Отчёты", reportsCmd),
            new("Персональные данные", personalCmd),
            new("Профподготовка", qualificationCmd),
            new("Назначения", appointmentsCmd),
            new("Отпуска", vacationCmd),
            new("Пенсионный возраст", pensionCmd),
            new("Настройки", settingsCmd),
            new("Уведомления", notificationCmd)
        };
        _notificationsViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(NotificationsViewModel.UnreadCount))
            {
                var notifItem = NavigationItems.FirstOrDefault(i => i.Title == "Уведомления");
                if (notifItem != null)
                    notifItem.BadgeCount = _notificationsViewModel.UnreadCount;
            }
        };
        _employeesListViewModel = _employeesListVmFactory();
        _navigationStore.CurrentViewModel = _employeesListViewModel;
        SetActive("Список работников");
    }

    public ObservableObject? CurrentViewModel => _navigationStore.CurrentViewModel;
    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }

    private void OnCurrentViewModelChanged() => OnPropertyChanged(nameof(CurrentViewModel));

    private void SetActive(string title)
    {
        foreach (var item in NavigationItems)
            item.IsActive = string.Equals(item.Title, title, StringComparison.Ordinal);
    }

    public async Task ShowStartupNotificationsAfterLoginAsync()
    {
        await _notificationsViewModel.CheckAndShowStartupNotifications();
    }
}