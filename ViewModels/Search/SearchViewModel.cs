using Inspector.Models;
using Inspector.Models.Search;
using Inspector.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Inspector.ViewModels.Search;

public sealed class SearchViewModel : ObservableObject
{
    private readonly IEmployeeSearchService _searchService;
    public ICommand ExportToExcelCommand { get; }
    private readonly string _templatesFolder;
    private readonly DispatcherTimer _searchDebounceTimer;
    private readonly IExportService _exportService;
    private DataTable? _resultsTable;
    private string _newTemplateName = string.Empty;
    private string? _selectedTemplateName;
    private int _resultsCount;
    private bool _isLoading;
    private bool _isClearing;
    private bool _isApplyingTemplate;
    private bool _searchInProgress;
    private bool _isResultsCollapsed;
    private ObservableCollection<string> _templateNames = new();

    public SearchViewModel(IEmployeeSearchService searchService, IExportService exportService)
    {
        _searchService = searchService;
        _exportService = exportService;
        _templatesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Inspector", "SearchTemplates");
        Directory.CreateDirectory(_templatesFolder);

        var paramList = GetAllParameterDefs().Select(d => new SearchParameterViewModel(d)).ToList();

        _searchDebounceTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(350)
        };
        _searchDebounceTimer.Tick += (_, _) =>
        {
            _searchDebounceTimer.Stop();
            RunSearchAsync();
        };

        foreach (var p in paramList)
        {
            p.PropertyChanged += (_, e) =>
            {
                if (_isClearing || _isApplyingTemplate) return;
                if (e.PropertyName == nameof(SearchParameterViewModel.IsSelected))
                {
                    if (ClearCommand is RelayCommand clearRc) clearRc.RaiseCanExecuteChanged();
                    if (SaveTemplateCommand is RelayCommand saveRc) saveRc.RaiseCanExecuteChanged();
                    RunSearchAsync();
                }
                else if (e.PropertyName == nameof(SearchParameterViewModel.FilterValue))
                {
                    _searchDebounceTimer.Stop();
                    _searchDebounceTimer.Start();
                }
            };
        }

        Parameters = new ObservableCollection<SearchParameterViewModel>(paramList);

        var fioParameter = Parameters.FirstOrDefault(p => p.Key == "Fio");
        if (fioParameter != null)
            fioParameter.IsSelected = true;

        ClearCommand = new RelayCommand(OnClear, CanClear);
        SaveTemplateCommand = new RelayCommand(OnSaveTemplate, CanSaveTemplate);
        LoadTemplateCommand = new RelayCommand(OnLoadTemplate, CanLoadTemplate);
        DeleteTemplateCommand = new RelayCommand(OnDeleteTemplate, CanDeleteTemplate);
        RefreshTemplatesCommand = new RelayCommand(RefreshTemplateList);
        ToggleResultsCollapsedCommand = new RelayCommand(() => IsResultsCollapsed = !IsResultsCollapsed);
        ExportToExcelCommand = new AsyncRelayCommand(ExportToExcelAsync);

        RefreshTemplateList();
    }

    public string Title => "Поиск";
    public ObservableCollection<SearchParameterViewModel> Parameters { get; }
    public DataTable? ResultsTable
    {
        get => _resultsTable;
        private set
        {
            if (SetProperty(ref _resultsTable, value))
            {
                ResultsCount = value?.Rows.Count ?? 0;
            }
        }
    }

    public int ResultsCount
    {
        get => _resultsCount;
        private set => SetProperty(ref _resultsCount, value);
    }

  
    public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }
    public string NewTemplateName
    {
        get => _newTemplateName;
        set
        {
            if (SetProperty(ref _newTemplateName, value ?? string.Empty))
                ((RelayCommand)SaveTemplateCommand).RaiseCanExecuteChanged();
        }
    }
    public string? SelectedTemplateName
    {
        get => _selectedTemplateName;
        set
        {
            if (SetProperty(ref _selectedTemplateName, value))
            {
                ((RelayCommand)LoadTemplateCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteTemplateCommand).RaiseCanExecuteChanged();
            }
        }
    }
    public ObservableCollection<string> TemplateNames
    {
        get => _templateNames;
        private set => SetProperty(ref _templateNames, value);
    }
    public bool IsResultsCollapsed
    {
        get => _isResultsCollapsed;
        set
        {
            if (SetProperty(ref _isResultsCollapsed, value))
            {
                OnPropertyChanged(nameof(IsFiltersPanelVisible));
                OnPropertyChanged(nameof(ToggleResultsButtonText));
            }
        }
    }
    public bool IsFiltersPanelVisible => !IsResultsCollapsed;
    public string ToggleResultsButtonText => IsResultsCollapsed ? "Свернуть" : "Развернуть";
    public ICommand ClearCommand { get; }
    public ICommand SaveTemplateCommand { get; }
    public ICommand LoadTemplateCommand { get; }
    public ICommand DeleteTemplateCommand { get; }
    public ICommand RefreshTemplatesCommand { get; }
    public ICommand ToggleResultsCollapsedCommand { get; }

    private static List<SearchParameterDef> GetAllParameterDefs()
    {
        return new List<SearchParameterDef>
        {
            new("Fio", "Данные работника", "ФИО", SearchFilterType.Text),
            new("IsMale", "Данные работника", "Пол", SearchFilterType.Boolean),
            new("BirthDate", "Данные работника", "Дата рождения", SearchFilterType.Date),
            new("Address", "Данные работника", "Адрес", SearchFilterType.Text),
            new("Phone", "Данные работника", "Телефон", SearchFilterType.Text),
            new("HomePhone", "Данные работника", "Домашний телефон", SearchFilterType.Text),
            new("IsUnionMember", "Данные работника", "Член профсоюза", SearchFilterType.Boolean),
            new("BirthPlace", "Данные работника", "Место рождения", SearchFilterType.Text),
            new("HireDate", "Данные работника", "Дата трудоустройства", SearchFilterType.Date),
            new("ContractEndDate", "Данные работника", "Срок действия контракта", SearchFilterType.Date),
            new("Category", "Данные работника", "Категория", SearchFilterType.Text),
            new("CurrentPosition", "Данные работника", "Текущая должность", SearchFilterType.Text),
            new("DiplomaQualification", "Данные работника", "Квалификация по диплому", SearchFilterType.Text),
            new("DiplomaSpecialty", "Данные работника", "Специальность по диплому", SearchFilterType.Text),
            new("SpecialtyExperienceYears", "Данные работника", "Стаж работы по специальности", SearchFilterType.Number),
            new("MainProfession", "Данные работника", "Основная профессия", SearchFilterType.Text),
            new("TotalExperienceYears", "Данные работника", "Общий стаж работы", SearchFilterType.Number),
            new("LastWorkPlace", "Данные работника", "Последнее место работы", SearchFilterType.Text),
            new("LastWorkPosition", "Данные работника", "Должность на последнем месте работы", SearchFilterType.Text),
            new("LastWorkDismissalDate", "Данные работника", "Дата увольнения с последн места раб", SearchFilterType.Date),
            new("LastWorkDismissalReason", "Данные работника", "Причина увольнения с последн места раб", SearchFilterType.Text),
            new("AdditionalInfo", "Данные работника", "Дополнительные сведения", SearchFilterType.Text),

            new("PassportSeries", "Паспорт", "Серия паспорта", SearchFilterType.Text),
            new("PassportNumber", "Паспорт", "Номер паспорта", SearchFilterType.Text),
            new("PassportIdentificationNumber", "Паспорт", "Идентификационный номер", SearchFilterType.Text),
            new("PassportIssuedBy", "Паспорт", "Кем выдан паспорт", SearchFilterType.Text),
            new("PassportIssueDate", "Паспорт", "Дата выдачи паспорта", SearchFilterType.Date),
            new("PassportExpiryDate", "Паспорт", "Паспорт действителен до", SearchFilterType.Date),

            new("EduInstitutionName", "Образование", "Название учебного заведения", SearchFilterType.Text),
            new("EduGraduationDate", "Образование", "Дата окончания учебного заведения", SearchFilterType.Date),
            new("EduStudyType", "Образование", "Вид обучения", SearchFilterType.Text),
            new("EduLevel", "Образование", "Уровень образования", SearchFilterType.Text),
            new("EduSpecialty", "Образование", "Специальность (образование)", SearchFilterType.Text),
            new("EduQualification", "Образование", "Квалификация (образование)", SearchFilterType.Text),

            new("MilAccountingGroup", "Воинский учет", "Группа учета", SearchFilterType.Text),
            new("MilAccountingCategory", "Воинский учет", "Категория учета", SearchFilterType.Text),
            new("MilComposition", "Воинский учет", "Состав", SearchFilterType.Text),
            new("MilRank", "Воинский учет", "Воинское звание", SearchFilterType.Text),
            new("MilSpecialty", "Воинский учет", "Военно-учетная специальность", SearchFilterType.Text),
            new("MilFitnessCategory", "Воинский учет", "Годность к военной службе", SearchFilterType.Text),
            new("MilCommissariatName", "Воинский учет", "Название райвоенкомата", SearchFilterType.Text),
            new("MilSpecialNumber", "Воинский учет", "Номер состава на спецучете", SearchFilterType.Text),

            new("AppointmentDate", "Назначения и перемещения", "Дата назначения", SearchFilterType.Date),
            new("AppointmentContractTerm", "Назначения и перемещения", "Период (назначения)", SearchFilterType.Text),
            new("AppointmentPosition", "Назначения и перемещения", "Должность (назначения)", SearchFilterType.Text),
            new("AppointmentContractType", "Назначения и перемещения", "Вид договора", SearchFilterType.Text),
            new("AppointmentBasisName", "Назначения и перемещения", "Документ основание", SearchFilterType.Text),
            new("AppointmentBasisDate", "Назначения и перемещения", "Дата документа", SearchFilterType.Date),
            new("AppointmentBasisNumber", "Назначения и перемещения", "Номер основания", SearchFilterType.Text),

            new("UpgradeDate", "Повыш. квалиф.", "Дата начала (повыш. квалиф.)", SearchFilterType.Date),
            new("UpgradeEndDate", "Повыш. квалиф.", "Дата окончания (повыш. квалиф.)", SearchFilterType.Date),
            new("UpgradeDays", "Повыш. квалиф.", "Кол-во дней (повыш. квалиф.)", SearchFilterType.Number),
            new("UpgradePeriod", "Повыш. квалиф.", "Период (повыш. квалиф.)", SearchFilterType.Text),
            new("UpgradeType", "Повыш. квалиф.", "Вид повышения", SearchFilterType.Text),
            new("UpgradeCertificateDate", "Повыш. квалиф.", "Дата свидетельства", SearchFilterType.Date),
            new("UpgradeCertificateNumber", "Повыш. квалиф.", "Номер свидетельства", SearchFilterType.Text),

            new("RetrainingDate", "Переподготовка", "Дата начала (переподг.)", SearchFilterType.Date),
            new("RetrainingEndDate", "Переподготовка", "Дата окончания (переподг.)", SearchFilterType.Date),
            new("RetrainingDays", "Переподготовка", "Кол-во дней (переподг.)", SearchFilterType.Number),
            new("RetrainingPeriod", "Переподготовка", "Период (переподг.)", SearchFilterType.Text),
            new("RetrainingSpecialty", "Переподготовка", "Специальность (переподг.)", SearchFilterType.Text),
            new("RetrainingDiplomaDate", "Переподготовка", "Дата диплома (переподг.)", SearchFilterType.Date),
            new("RetrainingDiplomaNumber", "Переподготовка", "Номер диплома (переподг.)", SearchFilterType.Text),

            new("FamilyLastName", "Семейный учет", "Фамилия (чл. семьи)", SearchFilterType.Text),
            new("FamilyFirstName", "Семейный учет", "Имя (чл. семьи)", SearchFilterType.Text),
            new("FamilyMiddleName", "Семейный учет", "Отчество (чл. семьи)", SearchFilterType.Text),
            new("FamilyBirthYear", "Семейный учет", "Дата рождения (чл. семьи)", SearchFilterType.Number),
            new("FamilyRelationType", "Семейный учет", "Вид родства (чл. семьи)", SearchFilterType.Text),

            new("VacationStartDate", "Отпуск", "Дата начала отпуска", SearchFilterType.Date),
            new("VacationEndDate", "Отпуск", "Дата окончания отпуска", SearchFilterType.Date),
            new("VacationDays", "Отпуск", "Кол-во дней отпуска", SearchFilterType.Number),
            new("VacationPeriod", "Отпуск", "Период отпуска", SearchFilterType.Text),
            new("VacationKind", "Отпуск", "Вид отпуска", SearchFilterType.Text),
            new("VacationBasis", "Отпуск", "Основание отпуска", SearchFilterType.Text),

            new("AttestationDate", "Аттестация", "Дата аттестации", SearchFilterType.Date),
            new("AttestationDecision", "Аттестация", "Решение комиссии по аттестации", SearchFilterType.Text),
        };
    }

    private async void RunSearchAsync()
    {
        if (_searchInProgress) return;
        var selected = Parameters.Where(p => p.IsSelected).ToList();
        if (selected.Count == 0)
        {
            ResultsTable = null;
            return;
        }
        _searchInProgress = true;
        IsLoading = true;
        try
        {
            var employees = await _searchService.GetEmployeesForSearchAsync();
            if (_isClearing) return;
            var filtered = FilterEmployees(employees, selected);
            ResultsTable = BuildResultTable(filtered, selected);
        }
        finally
        {
            _searchInProgress = false;
            IsLoading = false;
        }
    }

    private static List<Employee> FilterEmployees(IReadOnlyList<Employee> employees, List<SearchParameterViewModel> selected)
    {
        var list = employees.AsEnumerable();

        foreach (var p in selected.Where(x => !string.IsNullOrWhiteSpace(x.FilterValue)))
        {
            var val = p.FilterValue.Trim();
            var dFmt = System.Globalization.CultureInfo.CurrentCulture;

            list = p.Key switch
            {
                "Fio" => list.Where(e => e.GetFullName().Contains(val, StringComparison.OrdinalIgnoreCase)),
                "IsMale" => list.Where(e => (e.IsMale ? "м да v" : "ж нет ;").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "BirthDate" => list.Where(e => e.BirthDate.ToString("d", dFmt).Contains(val, StringComparison.OrdinalIgnoreCase)),
                "BirthPlace" => list.Where(e => (e.BirthPlace ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "DiplomaQualification" => list.Where(e => (e.DiplomaQualification ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "HireDate" => list.Where(e => (e.HireDate?.ToString("d", dFmt) ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "IsUnionMember" => list.Where(e => (e.IsUnionMember ? "да" : "нет").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "Category" => list.Where(e => (e.Category ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "AdditionalInfo" => list.Where(e => (e.AdditionalInfo ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "DiplomaSpecialty" => list.Where(e => (e.DiplomaSpecialty ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "Address" => list.Where(e => (e.Address ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "HomePhone" => list.Where(e => (e.HomePhone ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "MainProfession" => list.Where(e => (e.MainProfession ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "Phone" => list.Where(e => (e.Phone ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "LastWorkDismissalDate" => list.Where(e => (e.LastWorkDismissalDate?.ToString("d", dFmt) ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "LastWorkDismissalReason" => list.Where(e => (e.LastWorkDismissalReason ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "SpecialtyExperienceYears" => list.Where(e => (e.SpecialtyExperienceYears?.ToString() ?? "").Contains(val) ||
                                                             (int.TryParse(val, out var n1) && e.SpecialtyExperienceYears == n1)),
                "TotalExperienceYears" => list.Where(e => (e.TotalExperienceYears?.ToString() ?? "").Contains(val) ||
                                                         (int.TryParse(val, out var n2) && e.TotalExperienceYears == n2)),
                "ContractEndDate" => list.Where(e => (e.ContractEndDate?.ToString("d", dFmt) ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "LastWorkPlace" => list.Where(e => (e.LastWorkPlace ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "LastWorkPosition" => list.Where(e => (e.LastWorkPosition ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
    
                "PassportSeries" => list.Where(e => (e.Passport?.Series ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "PassportNumber" => list.Where(e => (e.Passport?.Number ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "PassportIdentificationNumber" => list.Where(e => (e.Passport?.IdentificationNumber ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "PassportIssuedBy" => list.Where(e => (e.Passport?.IssuedBy ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "PassportIssueDate" => list.Where(e => (e.Passport?.IssueDate?.ToString("d", dFmt) ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "PassportExpiryDate" => list.Where(e => (e.Passport?.ExpiryDate?.ToString("d", dFmt) ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),

                "EduInstitutionName" => list.Where(e => e.EducationRecords.Any(er => (er.InstitutionName ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "EduGraduationDate" => list.Where(e => e.EducationRecords.Any(er => (er.GraduationDate?.ToString("d", dFmt) ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "EduStudyType" => list.Where(e => e.EducationRecords.Any(er => (er.StudyType ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "EduSpecialty" => list.Where(e => e.EducationRecords.Any(er => (er.Specialty ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "EduQualification" => list.Where(e => e.EducationRecords.Any(er => (er.Qualification ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "EduLevel" => list.Where(e => e.EducationRecords.Any(er => (er.EducationLevel?.Name ?? er.CustomEducationLevel ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),

                "MilAccountingGroup" => list.Where(e => (e.MilitaryRegistration?.AccountingGroup ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "MilAccountingCategory" => list.Where(e => (e.MilitaryRegistration?.AccountingCategory ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "MilComposition" => list.Where(e => (e.MilitaryRegistration?.Composition ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "MilRank" => list.Where(e => (e.MilitaryRegistration?.MilitaryRank ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "MilSpecialty" => list.Where(e => (e.MilitaryRegistration?.MilitarySpecialty ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "MilFitnessCategory" => list.Where(e => (e.MilitaryRegistration?.FitnessCategory ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "MilCommissariatName" => list.Where(e => (e.MilitaryRegistration?.CommissariatName ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),
                "MilSpecialNumber" => list.Where(e => (e.MilitaryRegistration?.SpecialAccountingNumber ?? "").Contains(val, StringComparison.OrdinalIgnoreCase)),

                "AppointmentDate" => list.Where(e => e.AppointmentTransfers.Any(a => a.Date.HasValue && a.Date.Value.ToString("d", dFmt).Contains(val, StringComparison.OrdinalIgnoreCase))),
                "AppointmentPosition" => list.Where(e => e.AppointmentTransfers.Any(a => (a.Position ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "AppointmentBasisName" => list.Where(e => e.AppointmentTransfers.Any(a => (a.BasisDocumentName ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "AppointmentBasisDate" => list.Where(e => e.AppointmentTransfers.Any(a => (a.DocumentDate.HasValue ? a.DocumentDate.Value.ToString("d", dFmt) : "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "AppointmentBasisNumber" => list.Where(e => e.AppointmentTransfers.Any(a => (a.BasisDocumentNumber ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "AppointmentContractType" => list.Where(e => e.AppointmentTransfers.Any(a => (a.ContractType ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "AppointmentContractTerm" => list.Where(e => e.AppointmentTransfers.Any(a => (a.ContractTerm ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),

                "UpgradeDate" => list.Where(e => e.QualificationUpgrades.Any(u => u.StartDate.HasValue && u.StartDate.Value.ToString("d", dFmt).Contains(val, StringComparison.OrdinalIgnoreCase))),
                "UpgradeEndDate" => list.Where(e => e.QualificationUpgrades.Any(u => u.EndDate.HasValue && u.EndDate.Value.ToString("d", dFmt).Contains(val, StringComparison.OrdinalIgnoreCase))),
                "UpgradeDays" => list.Where(e => e.QualificationUpgrades.Any(u => (u.DaysCount?.ToString() ?? "").Contains(val) || (int.TryParse(val, out var n6) && u.DaysCount == n6))),
                "UpgradePeriod" => list.Where(e => e.QualificationUpgrades.Any(u =>
                {
                    if (!u.StartDate.HasValue || !u.DaysCount.HasValue) return false;
                    var start = u.StartDate.Value;
                    var end = start.AddDays(u.DaysCount.Value - 1);
                    return $"{start:dd.MM.yyyy}-{end:dd.MM.yyyy}".Contains(val, StringComparison.OrdinalIgnoreCase);
                })),
                "UpgradeType" => list.Where(e => e.QualificationUpgrades.Any(u => (u.UpgradeType ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "UpgradeCertificateDate" => list.Where(e => e.QualificationUpgrades.Any(u => (u.CertificateDate?.ToString("d", dFmt) ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "UpgradeCertificateNumber" => list.Where(e => e.QualificationUpgrades.Any(u => (u.CertificateNumber ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),

                "RetrainingDate" => list.Where(e => e.RetrainingRecords.Any(r => r.StartDate.HasValue && r.StartDate.Value.ToString("d", dFmt).Contains(val, StringComparison.OrdinalIgnoreCase))),
                "RetrainingEndDate" => list.Where(e => e.RetrainingRecords.Any(r => r.EndDate.HasValue && r.EndDate.Value.ToString("d", dFmt).Contains(val, StringComparison.OrdinalIgnoreCase))),
                "RetrainingDays" => list.Where(e => e.RetrainingRecords.Any(r => (r.DaysCount?.ToString() ?? "").Contains(val) || (int.TryParse(val, out var n5) && r.DaysCount == n5))),
                "RetrainingPeriod" => list.Where(e => e.RetrainingRecords.Any(r =>
                {
                    if (!r.StartDate.HasValue || !r.DaysCount.HasValue) return false;
                    var start = r.StartDate.Value;
                    var end = start.AddDays(r.DaysCount.Value - 1);
                    return $"{start:dd.MM.yyyy}-{end:dd.MM.yyyy}".Contains(val, StringComparison.OrdinalIgnoreCase);
                })),
                "RetrainingSpecialty" => list.Where(e => e.RetrainingRecords.Any(r => (r.Specialty ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "RetrainingDiplomaDate" => list.Where(e => e.RetrainingRecords.Any(r => (r.DiplomaDate?.ToString("d", dFmt) ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "RetrainingDiplomaNumber" => list.Where(e => e.RetrainingRecords.Any(r => (r.DiplomaNumber ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),

                "FamilyLastName" => list.Where(e => e.FamilyMembers.Any(f => (f.LastName ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "FamilyFirstName" => list.Where(e => e.FamilyMembers.Any(f => (f.FirstName ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "FamilyMiddleName" => list.Where(e => e.FamilyMembers.Any(f => (f.MiddleName ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "FamilyBirthYear" => list.Where(e => e.FamilyMembers.Any(f => (f.BirthDate?.ToString("dd.MM.yyyy") ?? "").Contains(val, StringComparison.OrdinalIgnoreCase) ||
                (f.BirthDate?.Year.ToString() ?? "").Contains(val))),
                "FamilyRelationType" => list.Where(e => e.FamilyMembers.Any(f => f.RelationType != null &&
                    f.RelationType.Name.Contains(val, StringComparison.OrdinalIgnoreCase))),

                "VacationBasis" => list.Where(e => e.VacationRecords.Any(v => (v.Basis ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "VacationDays" => list.Where(e => e.VacationRecords.Any(v => (v.WorkingDays?.ToString() ?? "").Contains(val) ||
                                                                             (int.TryParse(val, out var n4) && v.WorkingDays == n4))),
                "VacationStartDate" => list.Where(e => e.VacationRecords.Any(v => (v.StartDate?.ToString("d", dFmt) ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),
                "VacationKind" => list.Where(e => e.VacationRecords.Any(v => (v.VacationKind ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),

                "AttestationDate" => list.Where(e => e.AttestationRecords.Any(a => a.Date.HasValue && a.Date.Value.ToString("d", dFmt).Contains(val, StringComparison.OrdinalIgnoreCase))),
                "AttestationDecision" => list.Where(e => e.AttestationRecords.Any(a => (a.CommissionDecision ?? "").Contains(val, StringComparison.OrdinalIgnoreCase))),

                _ => list
            };
        }

        return list.ToList();
    }

    private static DataTable BuildResultTable(List<Employee> employees, List<SearchParameterViewModel> selected)
    {
        var dt = new DataTable();
        foreach (var p in selected)
            dt.Columns.Add(p.DisplayName, typeof(string));

        foreach (var e in employees)
        {
            var row = dt.NewRow();
            for (var i = 0; i < selected.Count; i++)
            {
                row[i] = GetCellValue(e, selected[i].Key);
            }
            dt.Rows.Add(row);
        }
        return dt;
    }

    private static string GetCellValue(Employee e, string key)
    {
        var fmt = "d";
        return key switch
        {
            "Fio" => e.GetFullName(),
            "IsMale" => e.IsMale ? "М" : "Ж",
            "BirthDate" => e.BirthDate.ToString(fmt),
            "BirthPlace" => e.BirthPlace ?? "",
            "DiplomaQualification" => e.DiplomaQualification ?? "",
            "HireDate" => e.HireDate?.ToString(fmt) ?? "",
            "IsUnionMember" => e.IsUnionMember ? "Да" : "Нет",
            "Category" => e.Category ?? "",
            "AdditionalInfo" => e.AdditionalInfo ?? "",
            "DiplomaSpecialty" => e.DiplomaSpecialty ?? "",
            "Address" => e.Address ?? "",
            "HomePhone" => e.HomePhone ?? "",
            "MainProfession" => e.MainProfession ?? "",
            "Phone" => e.Phone ?? "",
            "SpecialtyExperienceYears" => e.SpecialtyExperienceYears?.ToString() ?? "",
            "TotalExperienceYears" => e.TotalExperienceYears?.ToString() ?? "",
            "ContractEndDate" => e.ContractEndDate?.ToString(fmt) ?? "",
            "LastWorkPlace" => e.LastWorkPlace ?? "",
            "LastWorkPosition" => e.LastWorkPosition ?? "",
            "CurrentPosition" => e.CurrentProfession ?? "",
            "LastWorkDismissalDate" => e.LastWorkDismissalDate?.ToString(fmt) ?? "",
            "LastWorkDismissalReason" => e.LastWorkDismissalReason ?? "",

            "PassportSeries" => e.Passport?.Series ?? "",
            "PassportNumber" => e.Passport?.Number ?? "",
            "PassportIdentificationNumber" => e.Passport?.IdentificationNumber ?? "",
            "PassportIssuedBy" => e.Passport?.IssuedBy ?? "",
            "PassportIssueDate" => e.Passport?.IssueDate?.ToString(fmt) ?? "",
            "PassportExpiryDate" => e.Passport?.ExpiryDate?.ToString(fmt) ?? "",

            "EduInstitutionName" => string.Join(Environment.NewLine, e.EducationRecords.Select(er => er.InstitutionName).Where(x => x is not null)!),
            "EduGraduationDate" => string.Join(Environment.NewLine, e.EducationRecords.Where(er => er.GraduationDate.HasValue).Select(er => er.GraduationDate!.Value.ToString(fmt))),
            "EduStudyType" => string.Join(Environment.NewLine, e.EducationRecords.Select(er => er.StudyType).Where(x => !string.IsNullOrEmpty(x))!),
            "EduLevel" => string.Join(Environment.NewLine, e.EducationRecords.Select(er => er.EducationLevel?.Name ?? er.CustomEducationLevel ?? "").Where(x => !string.IsNullOrWhiteSpace(x))),
            "EduSpecialty" => string.Join(Environment.NewLine, e.EducationRecords.Select(er => er.Specialty).Where(s => !string.IsNullOrWhiteSpace(s))),
            "EduQualification" => string.Join(Environment.NewLine, e.EducationRecords.Select(er => er.Qualification).Where(q => !string.IsNullOrWhiteSpace(q))),
           
            "MilAccountingGroup" => e.MilitaryRegistration?.AccountingGroup ?? "",
            "MilAccountingCategory" => e.MilitaryRegistration?.AccountingCategory ?? "",
            "MilComposition" => e.MilitaryRegistration?.Composition ?? "",
            "MilRank" => e.MilitaryRegistration?.MilitaryRank ?? "",
            "MilSpecialty" => e.MilitaryRegistration?.MilitarySpecialty ?? "",
            "MilFitnessCategory" => e.MilitaryRegistration?.FitnessCategory ?? "",
            "MilCommissariatName" => e.MilitaryRegistration?.CommissariatName ?? "",
            "MilSpecialNumber" => e.MilitaryRegistration?.SpecialAccountingNumber ?? "",

            "AppointmentDate" => e.AppointmentTransfers.OrderByDescending(a => a.Date).FirstOrDefault() is { Date: { } dt } ? dt.ToString(fmt) : "",
            "AppointmentPosition" => string.Join(Environment.NewLine, e.AppointmentTransfers.Select(a => a.Position).Where(x => x is not null)!),
            "AppointmentBasisName" => string.Join(Environment.NewLine, e.AppointmentTransfers.Select(a => a.BasisDocumentName).Where(x => x is not null)!),
            "AppointmentBasisDate" => string.Join(Environment.NewLine, e.AppointmentTransfers.Where(a => a.DocumentDate.HasValue).Select(a => a.DocumentDate!.Value.ToString(fmt))),
            "AppointmentBasisNumber" => string.Join(Environment.NewLine, e.AppointmentTransfers.Select(a => a.BasisDocumentNumber).Where(x => x is not null)!),
            "AppointmentContractType" => string.Join(Environment.NewLine, e.AppointmentTransfers.Select(a => a.ContractType).Where(x => x is not null)!),
            "AppointmentContractTerm" => string.Join(Environment.NewLine, e.AppointmentTransfers.Select(a => a.ContractTerm).Where(x => x is not null)!),

            "UpgradeDate" => string.Join(Environment.NewLine, e.QualificationUpgrades.Where(u => u.StartDate.HasValue).Select(u => u.StartDate!.Value.ToString(fmt))),
            "UpgradeEndDate" => string.Join(Environment.NewLine, e.QualificationUpgrades.Where(u => u.EndDate.HasValue).Select(u => u.EndDate!.Value.ToString(fmt))),
            "UpgradeDays" => string.Join(Environment.NewLine, e.QualificationUpgrades.Select(u => u.DaysCount?.ToString() ?? "")),
            "UpgradePeriod" => string.Join(Environment.NewLine, e.QualificationUpgrades.Select(u => u.Period).Where(x => x is not null)!),
            "UpgradeType" => string.Join(Environment.NewLine, e.QualificationUpgrades.Select(u => u.UpgradeType).Where(x => x is not null)!),
            "UpgradeCertificateDate" => string.Join(Environment.NewLine, e.QualificationUpgrades.Where(u => u.CertificateDate.HasValue).Select(u => u.CertificateDate!.Value.ToString(fmt))),
            "UpgradeCertificateNumber" => string.Join(Environment.NewLine, e.QualificationUpgrades.Select(u => u.CertificateNumber).Where(x => x is not null)!),

            "RetrainingDate" => string.Join(Environment.NewLine, e.RetrainingRecords.Where(r => r.StartDate.HasValue).Select(r => r.StartDate!.Value.ToString(fmt))),
            "RetrainingEndDate" => string.Join(Environment.NewLine, e.RetrainingRecords.Where(r => r.EndDate.HasValue).Select(r => r.EndDate!.Value.ToString(fmt))),
            "RetrainingDays" => string.Join(Environment.NewLine, e.RetrainingRecords.Select(r => r.DaysCount?.ToString() ?? "")),
            "RetrainingPeriod" => string.Join(Environment.NewLine, e.RetrainingRecords.Select(r => r.Period).Where(x => x is not null)!),
            "RetrainingSpecialty" => string.Join(Environment.NewLine, e.RetrainingRecords.Select(r => r.Specialty).Where(x => x is not null)!),
            "RetrainingDiplomaDate" => string.Join(Environment.NewLine, e.RetrainingRecords.Where(r => r.DiplomaDate.HasValue).Select(r => r.DiplomaDate!.Value.ToString(fmt))),
            "RetrainingDiplomaNumber" => string.Join(Environment.NewLine, e.RetrainingRecords.Select(r => r.DiplomaNumber).Where(x => x is not null)!),

            "FamilyLastName" => string.Join(Environment.NewLine, e.FamilyMembers.Select(f => f.LastName).Where(x => !string.IsNullOrEmpty(x))!),
            "FamilyFirstName" => string.Join(Environment.NewLine, e.FamilyMembers.Select(f => f.FirstName).Where(x => !string.IsNullOrEmpty(x))!),
            "FamilyMiddleName" => string.Join(Environment.NewLine, e.FamilyMembers.Select(f => f.MiddleName).Where(x => !string.IsNullOrEmpty(x))!),
            "FamilyBirthYear" => string.Join(Environment.NewLine, e.FamilyMembers.Select(f => f.BirthDate?.ToString("dd.MM.yyyy") ?? "")),
            "FamilyRelationType" => string.Join(Environment.NewLine, e.FamilyMembers.Where(f => f.RelationType != null).Select(f => f.RelationType!.Name).Where(name => !string.IsNullOrEmpty(name))),

            "VacationPeriod" => string.Join(Environment.NewLine, e.VacationRecords
                .Where(v => v.StartDate.HasValue && v.WorkingDays.HasValue)
                .Select(v =>
                {
                    var start = v.StartDate!.Value;
                    var end = start.AddDays(v.WorkingDays!.Value - 1);
                    return $"{start:dd.MM.yyyy}-{end:dd.MM.yyyy}";
                })),
            "VacationBasis" => string.Join(Environment.NewLine, e.VacationRecords.Select(v => v.Basis).Where(x => x is not null)!),
            "VacationDays" => string.Join(Environment.NewLine, e.VacationRecords.Select(v => v.WorkingDays?.ToString() ?? "")),
            "VacationStartDate" => string.Join(Environment.NewLine, e.VacationRecords.Where(v => v.StartDate.HasValue).Select(v => v.StartDate!.Value.ToString(fmt))),
            "VacationEndDate" => string.Join(Environment.NewLine, e.VacationRecords.Where(v => v.EndDate.HasValue).Select(v => v.EndDate!.Value.ToString(fmt))),
            "VacationKind" => string.Join(Environment.NewLine, e.VacationRecords.Select(v => v.VacationKind).Where(x => x is not null)!),

            "AttestationDate" => string.Join(Environment.NewLine, e.AttestationRecords.Where(a => a.Date.HasValue).Select(a => a.Date!.Value.ToString(fmt))),
            "AttestationDecision" => string.Join(Environment.NewLine, e.AttestationRecords.Select(a => a.CommissionDecision).Where(x => x is not null)!),

            _ => ""
        };
    }

    private bool CanClear() => Parameters.Any(p => p.IsSelected);
    private void OnClear()
    {
        _searchDebounceTimer.Stop();
        _isClearing = true;
        try
        {
            foreach (var p in Parameters)
            {
                p.IsSelected = false;
                p.FilterValue = string.Empty;
            }
            ResultsTable = null;
        }
        finally
        {
            _isClearing = false;
        }
        ((RelayCommand)ClearCommand).RaiseCanExecuteChanged();
        ((RelayCommand)SaveTemplateCommand).RaiseCanExecuteChanged();
    }

    private bool CanSaveTemplate() => !string.IsNullOrWhiteSpace(NewTemplateName) && Parameters.Any(p => p.IsSelected);
    private void OnSaveTemplate()
    {
        var name = NewTemplateName.Trim();
        if (string.IsNullOrEmpty(name)) return;
        var template = new SearchTemplate
        {
            SelectedKeys = Parameters.Where(p => p.IsSelected).Select(p => p.Key).ToList(),
            FilterValues = Parameters.Where(p => p.IsSelected && !string.IsNullOrEmpty(p.FilterValue))
                .ToDictionary(p => p.Key, p => p.FilterValue)
        };
        var path = Path.Combine(_templatesFolder, SanitizeFileName(name) + ".json");
        File.WriteAllText(path, JsonSerializer.Serialize(template));
        RefreshTemplateList();
        NewTemplateName = string.Empty;
        ((RelayCommand)SaveTemplateCommand).RaiseCanExecuteChanged();
    }

    private bool CanLoadTemplate() => !string.IsNullOrEmpty(SelectedTemplateName);
    private bool CanDeleteTemplate() => !string.IsNullOrEmpty(SelectedTemplateName);
    private void OnDeleteTemplate()
    {
        if (string.IsNullOrEmpty(SelectedTemplateName)) return;
        var path = GetTemplatePathByName(SelectedTemplateName);
        if (path != null && File.Exists(path))
        {
            File.Delete(path);
            SelectedTemplateName = null;
            RefreshTemplateList();
        }
    }

    private void OnLoadTemplate()
    {
        if (string.IsNullOrEmpty(SelectedTemplateName)) return;
        var path = GetTemplatePathByName(SelectedTemplateName);
        if (path == null) return;
        var json = File.ReadAllText(path);
        var template = JsonSerializer.Deserialize<SearchTemplate>(json);
        if (template == null) return;
        _searchDebounceTimer.Stop();
        _isApplyingTemplate = true;
        try
        {
            foreach (var p in Parameters)
            {
                p.IsSelected = template.SelectedKeys.Contains(p.Key);
                p.FilterValue = template.FilterValues.TryGetValue(p.Key, out var v) ? v : string.Empty;
            }
        }
        finally
        {
            _isApplyingTemplate = false;
        }
        ((RelayCommand)ClearCommand).RaiseCanExecuteChanged();
        ((RelayCommand)SaveTemplateCommand).RaiseCanExecuteChanged();
        RunSearchAsync();
    }

    private void RefreshTemplateList()
    {
        if (!Directory.Exists(_templatesFolder))
        {
            TemplateNames = new ObservableCollection<string>();
            return;
        }
        var names = Directory.GetFiles(_templatesFolder, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => n != null)
            .OrderBy(n => n)
            .Cast<string>()
            .ToList();
        TemplateNames = new ObservableCollection<string>(names);
    }

    private string? GetTemplatePathByName(string displayName)
    {
        if (!Directory.Exists(_templatesFolder)) return null;
        var files = Directory.GetFiles(_templatesFolder, "*.json");
        return files.FirstOrDefault(f => string.Equals(Path.GetFileNameWithoutExtension(f), displayName, StringComparison.Ordinal));
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }

    private async Task ExportToExcelAsync()
    {
        if (ResultsTable == null || ResultsTable.Rows.Count == 0)
        {
            MessageBox.Show("Нет данных для экспорта.", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new SaveFileDialog
        {
            FileName = "Результаты_поиска",
            DefaultExt = ".xlsx",
            Filter = "Файлы Excel (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*",
            Title = "Сохранить результаты поиска",
            InitialDirectory = string.IsNullOrEmpty(Properties.Settings.Default.LastExportFolder)
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : Properties.Settings.Default.LastExportFolder,

            RestoreDirectory = true
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            if (File.Exists(dialog.FileName))
            {
                try
                {
                    File.Delete(dialog.FileName);
                }
                catch (IOException)
                {
                    MessageBox.Show(
                        $"Файл «{Path.GetFileName(dialog.FileName)}» уже открыт в Excel или другой программе.\n\n" +
                        "Закройте файл и нажмите кнопку экспорта ещё раз.",
                        "Файл занят",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }

            var selectedParams = Parameters
                .Where(p => p.IsSelected)
                .ToList();

            await _exportService.ExportSearchResultsToExcelAsync(ResultsTable, selectedParams, dialog.FileName);
            Properties.Settings.Default.LastExportFolder = Path.GetDirectoryName(dialog.FileName);
            Properties.Settings.Default.Save();
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = dialog.FileName,   
                    UseShellExecute = true
                });
            }
            catch
            {
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при экспорте:\n{ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    
    }
}