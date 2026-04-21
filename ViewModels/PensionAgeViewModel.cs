using System.Windows.Input;
using Inspector.Models;
using Inspector.Services;

namespace Inspector.ViewModels;

public sealed class PensionAgeViewModel : ObservableObject
{
    private readonly IEmployeesService _employeesService;
    private readonly IPensionAgeSettingsService _pensionSettings;

    private int _menAge = 65;
    private int _womenAge = 60;
    private int _menReachedCount;
    private int _womenReachedCount;
    private bool _isConfigOpen;
    private string _configMenAgeText = "65";
    private string _configWomenAgeText = "60";
    private string _configErrorMessage = string.Empty;
    private int _currentSettingsId;
    private bool _isLoading;

    public PensionAgeViewModel(
        IEmployeesService employeesService,
        IPensionAgeSettingsService pensionSettings)
    {
        _employeesService = employeesService;
        _pensionSettings = pensionSettings;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        OpenConfigureCommand = new RelayCommand(OpenConfigure);
        SaveConfigCommand = new AsyncRelayCommand(SaveConfigAsync);
        CancelConfigCommand = new RelayCommand(CancelConfig);
    }

    public static string Title => "Настройка пенсионного возраста";

    public int MenAge { get => _menAge; private set => SetProperty(ref _menAge, value); }
    public int WomenAge { get => _womenAge; private set => SetProperty(ref _womenAge, value); }
    public int MenReachedCount { get => _menReachedCount; private set => SetProperty(ref _menReachedCount, value); }
    public int WomenReachedCount { get => _womenReachedCount; private set => SetProperty(ref _womenReachedCount, value); }

    public string MenAgeText => AgeYearsText(MenAge);
    public string WomenAgeText => AgeYearsText(WomenAge);

    public bool IsConfigOpen { get => _isConfigOpen; set => SetProperty(ref _isConfigOpen, value); }
    public string ConfigMenAgeText { get => _configMenAgeText; set => SetProperty(ref _configMenAgeText, value); }
    public string ConfigWomenAgeText { get => _configWomenAgeText; set => SetProperty(ref _configWomenAgeText, value); }
    public string ConfigErrorMessage { get => _configErrorMessage; private set => SetProperty(ref _configErrorMessage, value); }
    public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }

    public AsyncRelayCommand LoadCommand { get; }
    public ICommand OpenConfigureCommand { get; }
    public AsyncRelayCommand SaveConfigCommand { get; }
    public ICommand CancelConfigCommand { get; }

    private static string AgeYearsText(int years)
    {
        var n = years % 100;
        var m = years % 10;
        if (m == 1 && n != 11) return $"{years} год";
        if (m >= 2 && m <= 4 && (n < 10 || n >= 20)) return $"{years} года";
        return $"{years} лет";
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var settings = await _pensionSettings.GetSettingsAsync();
            if (settings != null)
            {
                _currentSettingsId = settings.Id;
                MenAge = settings.MenAge;
                WomenAge = settings.WomenAge;
            }
            else
            {
                _currentSettingsId = 0;
                MenAge = 65;
                WomenAge = 60;
            }

            var employees = await _employeesService.GetAllAsync();
            var menAge = MenAge;
            var womenAge = WomenAge;
            MenReachedCount = 0;
            WomenReachedCount = 0;
            foreach (var e in employees)
            {
                var age = GetAge(e.BirthDate);
                if (e.IsMale && age >= menAge) MenReachedCount++;
                if (!e.IsMale && age >= womenAge) WomenReachedCount++;
            }

            OnPropertyChanged(nameof(MenAgeText));
            OnPropertyChanged(nameof(WomenAgeText));
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static int GetAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }

    private const int MinAge = 40;
    private const int MaxAge = 100;

    private void OpenConfigure()
    {
        ConfigMenAgeText = MenAge.ToString();
        ConfigWomenAgeText = WomenAge.ToString();
        ConfigErrorMessage = string.Empty;
        IsConfigOpen = true;
    }

    private async Task SaveConfigAsync()
    {
        ConfigErrorMessage = string.Empty;
        if (!int.TryParse(ConfigMenAgeText, out var menAge))
        {
            ConfigErrorMessage = "Значение должно быть числом от 40 до 100.";
            return;
        }
        if (!int.TryParse(ConfigWomenAgeText, out var womenAge))
        {
            ConfigErrorMessage = "Значение должно быть числом от 40 до 100.";
            return;
        }
        if (menAge < MinAge || menAge > MaxAge || womenAge < MinAge || womenAge > MaxAge)
        {
            ConfigErrorMessage = "Возраст не может быть ниже 40 и выше 100.";
            return;
        }
        var settings = new PensionAgeSetting
        {
            Id = _currentSettingsId,
            MenAge = menAge,
            WomenAge = womenAge
        };
        await _pensionSettings.SaveSettingsAsync(settings);
        IsConfigOpen = false;
        await LoadAsync();
    }

    private void CancelConfig() => IsConfigOpen = false;
}
