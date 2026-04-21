using Inspector.Models;
using Inspector.Services;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;

namespace Inspector.ViewModels.Employees;

public sealed class EmployeeEditViewModel : ValidatableViewModelBase
{
    private readonly IEmployeesService _employeesService;
    private readonly Action<Employee> _onSaved;
    private readonly Action _onCancelled;
    private readonly Employee? _originalEmployee;

    private bool _isSaving;
    private string _category = string.Empty;
    private string _title = "Добавление";

    private string _lastName = string.Empty;
    private string _firstName = string.Empty;
    private string _middleName = string.Empty;
    private bool _isMale = true;
    private DateTime _birthDate = DateTime.Today.AddYears(-18);
    private string _birthPlace = string.Empty;
    private bool _isUnionMember;
    private string _address = string.Empty;
    private string _phone = string.Empty;
    private string _homePhone = string.Empty;
    private string _currentPosition = string.Empty;
    private string _diplomaSpecialty = string.Empty;
    private string _diplomaQualification = string.Empty;
    private string _mainProfession = string.Empty;
    private int? _specialtyExperienceYears;
    private int? _totalExperienceYears;
    private DateTime? _hireDate;
    private string _lastWorkPlace = string.Empty;
    private string _lastWorkPosition = string.Empty;
    private DateTime? _lastWorkDismissalDate;
    private string _lastWorkDismissalReason = string.Empty;
    private DateTime? _contractEndDate;
    private string _additionalInfo = string.Empty;

    private string _passportSeries = string.Empty;
    private string _passportNumber = string.Empty;
    private string _identificationNumber = string.Empty;
    private string _issuedBy = string.Empty;
    private DateTime? _issueDate;
    private DateTime? _passportExpiryDate;

    private string _milAccountingGroup = string.Empty;
    private string _milAccountingCategory = string.Empty;
    private string _milComposition = string.Empty;
    private string _milRank = string.Empty;
    private string _milSpecialty = string.Empty;
    private string _milFitnessCategory = string.Empty;
    private string _milCommissariatName = string.Empty;
    private string _milSpecialNumber = string.Empty;

    private readonly ObservableCollection<FamilyMember> _familyMembers = new();
    private readonly ObservableCollection<AttestationRecord> _attestationItems = new();
    private readonly ObservableCollection<QualificationUpgrade> _qualificationUpgradeItems = new();
    private readonly ObservableCollection<RetrainingRecord> _retrainingItems = new();
    private readonly ObservableCollection<EducationRecord> _educationItems = new();
    private readonly ObservableCollection<AppointmentTransfer> _appointmentItems = new();
    private readonly ObservableCollection<VacationRecord> _vacationItems = new();

    private readonly ObservableCollection<EducationLevel> _educationLevels = new();
    private readonly ObservableCollection<RelationType> _relationTypes = new();

    public EmployeeEditViewModel(
        IEmployeesService employeesService,
        Action<Employee> onSaved,
        Action onCancelled,
        Employee? employeeToEdit = null)
    {
        _employeesService = employeesService;
        _onSaved = onSaved;
        _onCancelled = onCancelled;
        _originalEmployee = employeeToEdit;

        Title = employeeToEdit == null ? "Добавление нового сотрудника" : "Редактирование сотрудника";

        SaveCommand = new AsyncRelayCommand(SaveAsync, () => !IsSaving);
        CancelCommand = new RelayCommand(() => _onCancelled());

        AddFamilyMemberCommand = new RelayCommand(AddFamilyMember);
        RemoveFamilyMemberCommand = new RelayCommand<object>(obj => RemoveFamilyMember(obj as FamilyMember));
        AddAttestationCommand = new RelayCommand(AddAttestation);
        RemoveAttestationCommand = new RelayCommand<object>(obj => RemoveAttestation(obj as AttestationRecord));
        AddQualificationUpgradeCommand = new RelayCommand(AddQualificationUpgrade);
        RemoveQualificationUpgradeCommand = new RelayCommand<object>(obj => RemoveQualificationUpgrade(obj as QualificationUpgrade));
        AddRetrainingCommand = new RelayCommand(AddRetraining);
        RemoveRetrainingCommand = new RelayCommand<object>(obj => RemoveRetraining(obj as RetrainingRecord));
        AddAppointmentCommand = new RelayCommand(AddAppointment);
        RemoveAppointmentCommand = new RelayCommand<object>(obj => RemoveAppointment(obj as AppointmentTransfer));
        AddEducationCommand = new RelayCommand(AddEducation);
        RemoveEducationCommand = new RelayCommand<object>(obj => RemoveEducation(obj as EducationRecord));
        AddVacationCommand = new RelayCommand(AddVacation);
        RemoveVacationCommand = new RelayCommand<object>(obj => RemoveVacation(obj as VacationRecord));

        _familyMembers.CollectionChanged += (_, _) => RaiseFamilySectionStatus();
        _attestationItems.CollectionChanged += (_, _) => RaiseAttestationSectionStatus();
        _qualificationUpgradeItems.CollectionChanged += (_, _) => RaiseUpgradeSectionStatus();
        _retrainingItems.CollectionChanged += (_, _) => RaiseRetrainingSectionStatus();
        _appointmentItems.CollectionChanged += (_, _) => RaiseAppointmentSectionStatus();
        _educationItems.CollectionChanged += (_, _) => RaiseEducationSectionStatus();
        _vacationItems.CollectionChanged += (_, _) => RaiseVacationSectionStatus();

        LoadRelationTypesAsync();

        if (employeeToEdit != null)
            LoadEducationLevelsAndEmployeeAsync(employeeToEdit);
        else
            LoadEducationLevelsAsync();

        ValidateAll();
        RaiseAllSectionStatuses();
    }


    private async void LoadEducationLevelsAndEmployeeAsync(Employee employeeToEdit)
    {
        try
        {
            var levels = await _employeesService.GetEducationLevelsAsync();

            _educationLevels.Clear();
            foreach (var level in levels)
                _educationLevels.Add(level);

            LoadExistingEmployee(employeeToEdit);

            RaiseEducationSectionStatus();
        }
        catch (Exception ex)
        {
        }
    }
    private async void LoadRelationTypesAsync()
    {
        try
        {
            var relationTypes = await _employeesService.GetRelationTypesAsync();
            foreach (var rt in relationTypes)
                _relationTypes.Add(rt);
        }
        catch (Exception ex)
        {
        }
    }
    private async void LoadEducationLevelsAsync(Employee? employeeToEdit = null)
    {
        try
        {
            var levels = await _employeesService.GetEducationLevelsAsync();
            _educationLevels.Clear();
            foreach (var level in levels)
                _educationLevels.Add(level);

            if (employeeToEdit != null)
            {
                LoadEducationLevelsAndEmployeeAsync(employeeToEdit);
                RaiseEducationSectionStatus();   
            }
        }
        catch (Exception ex)
        {
        }
    }


    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public bool IsSaving
    {
        get => _isSaving;
        private set
        {
            if (SetProperty(ref _isSaving, value))
                SaveCommand.RaiseCanExecuteChanged();
        }
    }

    public int GenderIndex
    {
        get => IsMale ? 0 : 1;
        set
        {
            if (SetProperty(ref _isMale, value == 0))
                RaiseMainSectionStatus();
        }
    }

    public string Category
    {
        get => _category;                    
        set
        {
            if (!SetProperty(ref _category, value)) return;
            RaiseMainSectionStatus();
        }
    }

    public string LastName
    {
        get => _lastName;
        set { if (!SetProperty(ref _lastName, value)) return; ValidateLastName(); RaiseMainSectionStatus(); }
    }

    public string FirstName
    {
        get => _firstName;
        set { if (!SetProperty(ref _firstName, value)) return; ValidateFirstName(); RaiseMainSectionStatus(); }
    }

    public string MiddleName
    {
        get => _middleName;
        set { if (!SetProperty(ref _middleName, value)) return; ValidateMiddleName(); RaiseMainSectionStatus(); }
    }

    public bool IsMale
    {
        get => _isMale;
        set { if (SetProperty(ref _isMale, value)) RaiseMainSectionStatus(); }
    }

    public DateTime BirthDate
    {
        get => _birthDate;
        set { if (!SetProperty(ref _birthDate, value)) return; ValidateBirthDate(); RaiseMainSectionStatus(); }
    }

    public string BirthPlace
    {
        get => _birthPlace;
        set { if (!SetProperty(ref _birthPlace, value)) return; ValidateBirthPlace(); RaiseMainSectionStatus(); }
    }

    public bool IsUnionMember
    {
        get => _isUnionMember;
        set { if (SetProperty(ref _isUnionMember, value)) RaiseMainSectionStatus(); }
    }

    public string Address
    {
        get => _address;
        set { if (!SetProperty(ref _address, value)) return; ValidateAddress(); RaiseMainSectionStatus(); }
    }

    public string Phone
    {
        get => _phone;
        set { if (!SetProperty(ref _phone, value)) return; ValidatePhone(); RaiseMainSectionStatus(); }
    }

    public string HomePhone
    {
        get => _homePhone;
        set { if (!SetProperty(ref _homePhone, value)) return; ValidateHomePhone(); RaiseMainSectionStatus(); }
    }

    public string CurrentPosition
    {
        get => _currentPosition;
        set
        {
            if (!SetProperty(ref _currentPosition, value)) return;
            RaiseMainSectionStatus();        
        }
    }

    public string DiplomaSpecialty
    {
        get => _diplomaSpecialty;
        set { if (SetProperty(ref _diplomaSpecialty, value)) RaiseMainSectionStatus(); }
    }

    public string DiplomaQualification
    {
        get => _diplomaQualification;
        set { if (SetProperty(ref _diplomaQualification, value)) RaiseMainSectionStatus(); }
    }

    public string MainProfession
    {
        get => _mainProfession;
        set { if (SetProperty(ref _mainProfession, value)) RaiseMainSectionStatus(); }
    }
    public int? SpecialtyExperienceYears
    {
        get => _specialtyExperienceYears;
        set { if (SetProperty(ref _specialtyExperienceYears, value)) RaiseMainSectionStatus(); }
    }

    public int? TotalExperienceYears
    {
        get => _totalExperienceYears;
        set { if (SetProperty(ref _totalExperienceYears, value)) RaiseMainSectionStatus(); }
    }

    public DateTime? HireDate
    {
        get => _hireDate;
        set { if (SetProperty(ref _hireDate, value)) RaiseMainSectionStatus(); }
    }

    public string LastWorkPlace
    {
        get => _lastWorkPlace;
        set { if (SetProperty(ref _lastWorkPlace, value)) RaiseMainSectionStatus(); }
    }

    public string LastWorkPosition
    {
        get => _lastWorkPosition;
        set { if (SetProperty(ref _lastWorkPosition, value)) RaiseMainSectionStatus(); }
    }

    public DateTime? LastWorkDismissalDate
    {
        get => _lastWorkDismissalDate;
        set { if (SetProperty(ref _lastWorkDismissalDate, value)) RaiseMainSectionStatus(); }
    }

    public string LastWorkDismissalReason
    {
        get => _lastWorkDismissalReason;
        set { if (SetProperty(ref _lastWorkDismissalReason, value)) RaiseMainSectionStatus(); }
    }

    public DateTime? ContractEndDate
    {
        get => _contractEndDate;
        set { if (SetProperty(ref _contractEndDate, value)) RaiseMainSectionStatus(); }
    }

    public string AdditionalInfo
    {
        get => _additionalInfo;
        set { if (SetProperty(ref _additionalInfo, value)) RaiseMainSectionStatus(); }
    }

    public bool IsMainSectionFullyFilled =>
        !string.IsNullOrWhiteSpace(LastName) &&
        !string.IsNullOrWhiteSpace(FirstName) &&
        BirthDate != default &&
        !string.IsNullOrWhiteSpace(BirthPlace) &&
        !string.IsNullOrWhiteSpace(Address) &&
        !string.IsNullOrWhiteSpace(Phone) &&
        !string.IsNullOrWhiteSpace(CurrentPosition);

    public bool IsMainSectionPartiallyFilled => !IsMainSectionFullyFilled &&
    (!string.IsNullOrWhiteSpace(LastName) ||
     !string.IsNullOrWhiteSpace(FirstName) ||
     !string.IsNullOrWhiteSpace(MiddleName) ||
     BirthDate != default ||
     !string.IsNullOrWhiteSpace(BirthPlace) ||
     !string.IsNullOrWhiteSpace(Address) ||
     !string.IsNullOrWhiteSpace(Phone) ||
     !string.IsNullOrWhiteSpace(HomePhone) ||
     !string.IsNullOrWhiteSpace(CurrentPosition) ||
     !string.IsNullOrWhiteSpace(DiplomaSpecialty) ||
     !string.IsNullOrWhiteSpace(DiplomaQualification) ||
     !string.IsNullOrWhiteSpace(MainProfession) ||
     !string.IsNullOrWhiteSpace(Category) ||
     SpecialtyExperienceYears.HasValue ||
     TotalExperienceYears.HasValue ||
     HireDate.HasValue ||
     !string.IsNullOrWhiteSpace(LastWorkPlace) ||
     !string.IsNullOrWhiteSpace(LastWorkPosition) ||
     LastWorkDismissalDate.HasValue ||
     !string.IsNullOrWhiteSpace(LastWorkDismissalReason) ||
     ContractEndDate.HasValue ||
     !string.IsNullOrWhiteSpace(AdditionalInfo) ||
     IsMale != true ||
     IsUnionMember);

    public string PassportSeries
    {
        get => _passportSeries;
        set { if (!SetProperty(ref _passportSeries, value)) return; ValidatePassportSeries(); RaisePassportSectionStatus(); }
    }

    public string PassportNumber
    {
        get => _passportNumber;
        set { if (!SetProperty(ref _passportNumber, value)) return; ValidatePassportNumber(); RaisePassportSectionStatus(); }
    }

    public string IdentificationNumber
    {
        get => _identificationNumber;
        set { if (!SetProperty(ref _identificationNumber, value)) return; ValidateIdentificationNumber(); RaisePassportSectionStatus(); }
    }

    public string IssuedBy
    {
        get => _issuedBy;
        set { if (!SetProperty(ref _issuedBy, value)) return; ValidateIssuedBy(); RaisePassportSectionStatus(); }
    }

    public DateTime? IssueDate
    {
        get => _issueDate;
        set { if (SetProperty(ref _issueDate, value)) RaisePassportSectionStatus(); }
    }

    public DateTime? PassportExpiryDate
    {
        get => _passportExpiryDate;
        set
        {
            if (SetProperty(ref _passportExpiryDate, value))
            {
                ValidatePassportExpiryDate();
                RaisePassportSectionStatus();
            }
                
        }
    }
    public bool IsPassportSectionFullyFilled =>
        !string.IsNullOrWhiteSpace(PassportSeries) && !string.IsNullOrWhiteSpace(PassportNumber) &&
        !string.IsNullOrWhiteSpace(IdentificationNumber) && !string.IsNullOrWhiteSpace(IssuedBy) && IssueDate.HasValue &&
        PassportExpiryDate.HasValue;

    public bool IsPassportSectionPartiallyFilled => !IsPassportSectionFullyFilled &&
        (!string.IsNullOrWhiteSpace(PassportSeries) || !string.IsNullOrWhiteSpace(PassportNumber) ||
         !string.IsNullOrWhiteSpace(IdentificationNumber) || !string.IsNullOrWhiteSpace(IssuedBy) || IssueDate.HasValue ||
         PassportExpiryDate.HasValue);

    public string MilAccountingGroup
    {
        get => _milAccountingGroup;
        set { if (SetProperty(ref _milAccountingGroup, value)) RaiseMilitarySectionStatus(); }
    }

    public string MilAccountingCategory
    {
        get => _milAccountingCategory;
        set { if (SetProperty(ref _milAccountingCategory, value)) RaiseMilitarySectionStatus(); }
    }

    public string MilComposition
    {
        get => _milComposition;
        set { if (SetProperty(ref _milComposition, value)) RaiseMilitarySectionStatus(); }
    }

    public string MilRank
    {
        get => _milRank;
        set { if (SetProperty(ref _milRank, value)) RaiseMilitarySectionStatus(); }
    }

    public string MilSpecialty
    {
        get => _milSpecialty;
        set { if (SetProperty(ref _milSpecialty, value)) RaiseMilitarySectionStatus(); }
    }

    public string MilFitnessCategory
    {
        get => _milFitnessCategory;
        set { if (SetProperty(ref _milFitnessCategory, value)) RaiseMilitarySectionStatus(); }
    }

    public string MilCommissariatName
    {
        get => _milCommissariatName;
        set { if (SetProperty(ref _milCommissariatName, value)) RaiseMilitarySectionStatus(); }
    }

    public string MilSpecialNumber
    {
        get => _milSpecialNumber;
        set { if (SetProperty(ref _milSpecialNumber, value)) RaiseMilitarySectionStatus(); }
    }

    public bool IsMilitarySectionFullyFilled =>
        !string.IsNullOrWhiteSpace(MilAccountingGroup) && !string.IsNullOrWhiteSpace(MilAccountingCategory) &&
        !string.IsNullOrWhiteSpace(MilComposition) && !string.IsNullOrWhiteSpace(MilRank) &&
        !string.IsNullOrWhiteSpace(MilSpecialty) && !string.IsNullOrWhiteSpace(MilFitnessCategory) &&
        !string.IsNullOrWhiteSpace(MilCommissariatName) && !string.IsNullOrWhiteSpace(MilSpecialNumber);

    public bool IsMilitarySectionPartiallyFilled => !IsMilitarySectionFullyFilled &&
        (!string.IsNullOrWhiteSpace(MilAccountingGroup) || !string.IsNullOrWhiteSpace(MilAccountingCategory) ||
         !string.IsNullOrWhiteSpace(MilComposition) || !string.IsNullOrWhiteSpace(MilRank) ||
         !string.IsNullOrWhiteSpace(MilSpecialty) || !string.IsNullOrWhiteSpace(MilFitnessCategory) ||
         !string.IsNullOrWhiteSpace(MilCommissariatName) || !string.IsNullOrWhiteSpace(MilSpecialNumber));
   
    
    public bool IsAppointmentSectionFullyFilled =>
        AppointmentItems.Count > 0 && AppointmentItems.All(a =>
            a.Date.HasValue &&
            !string.IsNullOrWhiteSpace(a.Position) &&
            !string.IsNullOrWhiteSpace(a.ContractType) &&
            !string.IsNullOrWhiteSpace(a.ContractTerm) &&           
            !string.IsNullOrWhiteSpace(a.BasisDocumentName) &&
            !string.IsNullOrWhiteSpace(a.BasisDocumentNumber) &&
            a.DocumentDate.HasValue);

    public bool IsAppointmentSectionPartiallyFilled => AppointmentItems.Any() && !IsAppointmentSectionFullyFilled;
    public bool IsEducationSectionFullyFilled =>
        EducationItems.Count > 0 && EducationItems.All(e =>
            (e.EducationLevel != null || !string.IsNullOrWhiteSpace(e.CustomEducationLevel)) &&
            !string.IsNullOrWhiteSpace(e.InstitutionName) &&
            !string.IsNullOrWhiteSpace(e.StudyType) &&
            !string.IsNullOrWhiteSpace(e.Specialty) &&    
            !string.IsNullOrWhiteSpace(e.Qualification));   

    public bool IsEducationSectionPartiallyFilled => EducationItems.Any() && !IsEducationSectionFullyFilled;

    public bool IsUpgradeSectionFullyFilled =>
        _qualificationUpgradeItems.Count > 0 && _qualificationUpgradeItems.All(q =>
            q.StartDate.HasValue && !string.IsNullOrWhiteSpace(q.UpgradeType) &&
            q.CertificateDate.HasValue && !string.IsNullOrWhiteSpace(q.CertificateNumber));

    public bool IsUpgradeSectionPartiallyFilled => _qualificationUpgradeItems.Any() && !IsUpgradeSectionFullyFilled;

    public bool IsRetrainingSectionFullyFilled =>
            _retrainingItems.Count > 0 && _retrainingItems.All(r =>
                r.StartDate.HasValue && !string.IsNullOrWhiteSpace(r.Specialty) &&
                r.DiplomaDate.HasValue && !string.IsNullOrWhiteSpace(r.DiplomaNumber));

    public bool IsRetrainingSectionPartiallyFilled => _retrainingItems.Any() && !IsRetrainingSectionFullyFilled;
    public bool IsFamilySectionFullyFilled =>
        _familyMembers.Count > 0 && _familyMembers.All(m =>
            !string.IsNullOrWhiteSpace(m.LastName) &&
            !string.IsNullOrWhiteSpace(m.FirstName) &&
            m.RelationType is not null);

    public bool IsFamilySectionPartiallyFilled => _familyMembers.Any() && !IsFamilySectionFullyFilled;

    public bool IsAttestationSectionFullyFilled =>
        AttestationItems.Count > 0 && AttestationItems.All(a =>
            a.Date.HasValue && !string.IsNullOrWhiteSpace(a.CommissionDecision));

    public bool IsAttestationSectionPartiallyFilled => AttestationItems.Any() && !IsAttestationSectionFullyFilled;

    public bool IsVacationSectionFullyFilled =>
        _vacationItems.Count > 0 && _vacationItems.All(v =>
            v.StartDate.HasValue && !string.IsNullOrWhiteSpace(v.VacationKind) &&
            !string.IsNullOrWhiteSpace(v.Basis) && v.WorkingDays.HasValue);

    public bool IsVacationSectionPartiallyFilled =>
        _vacationItems.Count > 0 && !IsVacationSectionFullyFilled &&
        _vacationItems.Any(v => v.StartDate.HasValue || !string.IsNullOrWhiteSpace(v.VacationKind) ||
                               !string.IsNullOrWhiteSpace(v.Basis) || v.WorkingDays.HasValue);

    private void RaiseMainSectionStatus() { OnPropertyChanged(nameof(IsMainSectionFullyFilled)); OnPropertyChanged(nameof(IsMainSectionPartiallyFilled)); }
    private void RaisePassportSectionStatus() { OnPropertyChanged(nameof(IsPassportSectionFullyFilled)); OnPropertyChanged(nameof(IsPassportSectionPartiallyFilled)); }
    private void RaiseMilitarySectionStatus() { OnPropertyChanged(nameof(IsMilitarySectionFullyFilled)); OnPropertyChanged(nameof(IsMilitarySectionPartiallyFilled)); }
    private void RaiseAppointmentSectionStatus() { OnPropertyChanged(nameof(IsAppointmentSectionFullyFilled)); OnPropertyChanged(nameof(IsAppointmentSectionPartiallyFilled)); }
    private void RaiseEducationSectionStatus() { OnPropertyChanged(nameof(IsEducationSectionFullyFilled)); OnPropertyChanged(nameof(IsEducationSectionPartiallyFilled)); }
    private void RaiseUpgradeSectionStatus() { OnPropertyChanged(nameof(IsUpgradeSectionFullyFilled)); OnPropertyChanged(nameof(IsUpgradeSectionPartiallyFilled)); }
    private void RaiseRetrainingSectionStatus() { OnPropertyChanged(nameof(IsRetrainingSectionFullyFilled)); OnPropertyChanged(nameof(IsRetrainingSectionPartiallyFilled)); }
    private void RaiseFamilySectionStatus() { OnPropertyChanged(nameof(IsFamilySectionFullyFilled)); OnPropertyChanged(nameof(IsFamilySectionPartiallyFilled)); }
    private void RaiseAttestationSectionStatus() { OnPropertyChanged(nameof(IsAttestationSectionFullyFilled)); OnPropertyChanged(nameof(IsAttestationSectionPartiallyFilled)); }
    private void RaiseVacationSectionStatus() { OnPropertyChanged(nameof(IsVacationSectionFullyFilled)); OnPropertyChanged(nameof(IsVacationSectionPartiallyFilled)); }
    private void ValidatePassportExpiryDate() { ClearErrors(nameof(PassportExpiryDate)); 
        if (PassportExpiryDate.HasValue && IssueDate.HasValue)
        {
            if (PassportExpiryDate.Value <= IssueDate.Value)
            {
                AddError(nameof(PassportExpiryDate), "Дата окончания паспорта должна быть позже даты выдачи.");
            }
        }
    }
    private void RaiseAllSectionStatuses()
    {
        RaiseMainSectionStatus();
        RaisePassportSectionStatus();
        RaiseMilitarySectionStatus();
        RaiseAppointmentSectionStatus();
        RaiseEducationSectionStatus();
        RaiseUpgradeSectionStatus();
        RaiseRetrainingSectionStatus();
        RaiseFamilySectionStatus();
        RaiseAttestationSectionStatus();
        RaiseVacationSectionStatus();
    }

    private void AddEducation()
    {
        var r = new EducationRecord();
        r.PropertyChanged += (_, _) => RaiseEducationSectionStatus();
        _educationItems.Add(r);
        RaiseEducationSectionStatus();
    }

    private void RemoveEducation(EducationRecord? r)
    {
        if (r is null) return;
        _educationItems.Remove(r);
        RaiseEducationSectionStatus();
    }

    private void AddVacation()
    {
        var r = new VacationRecord();
        r.PropertyChanged += (_, _) => RaiseVacationSectionStatus();
        _vacationItems.Add(r);
        RaiseVacationSectionStatus();
    }

    private void RemoveVacation(VacationRecord? r)
    {
        if (r is null) return;
        _vacationItems.Remove(r);
        RaiseVacationSectionStatus();
    }

    private void AddAttestation()
    {
        var r = new AttestationRecord();
        r.PropertyChanged += (_, _) => RaiseAttestationSectionStatus();
        _attestationItems.Add(r);
        RaiseAttestationSectionStatus();
    }

    private void RemoveAttestation(object? obj)
    {
        if (obj is AttestationRecord r)
        {
            _attestationItems.Remove(r);
            RaiseAttestationSectionStatus();
        }
    }

    private void AddQualificationUpgrade()
    {
        var r = new QualificationUpgrade();
        r.PropertyChanged += (_, _) => RaiseUpgradeSectionStatus();
        _qualificationUpgradeItems.Add(r);
        RaiseUpgradeSectionStatus();
    }

    private void RemoveQualificationUpgrade(object? obj)
    {
        if (obj is QualificationUpgrade r)
        {
            _qualificationUpgradeItems.Remove(r);
            RaiseUpgradeSectionStatus();
        }
    }

    private void AddRetraining()
    {
        var r = new RetrainingRecord();
        r.PropertyChanged += (_, _) => RaiseRetrainingSectionStatus();
        _retrainingItems.Add(r);
        RaiseRetrainingSectionStatus();
    }

    private void RemoveRetraining(object? obj)
    {
        if (obj is RetrainingRecord r)
        {
            _retrainingItems.Remove(r);
            RaiseRetrainingSectionStatus();
        }
    }

    private void AddFamilyMember()
    {
        var r = new FamilyMember();
        r.PropertyChanged += (_, _) => RaiseFamilySectionStatus();
        _familyMembers.Add(r);
        RaiseFamilySectionStatus();
    }

    private void RemoveFamilyMember(FamilyMember? r)
    {
        if (r is null) return;
        _familyMembers.Remove(r);
        RaiseFamilySectionStatus();
    }

    private void AddAppointment()
    {
        var r = new AppointmentTransfer();

        r.PropertyChanged += (_, _) => RaiseAppointmentSectionStatus();

        _appointmentItems.Add(r);
        RaiseAppointmentSectionStatus();   
    }

    private void RemoveAppointment(object? obj)
    {
        if (obj is AppointmentTransfer r)
        {
            _appointmentItems.Remove(r);
            RaiseAppointmentSectionStatus();
        }
    }

    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand AddFamilyMemberCommand { get; }
    public RelayCommand<object> RemoveFamilyMemberCommand { get; }
    public RelayCommand AddVacationCommand { get; }
    public RelayCommand<object> RemoveVacationCommand { get; }
    public RelayCommand AddAttestationCommand { get; }
    public RelayCommand<object> RemoveAttestationCommand { get; }
    public RelayCommand AddQualificationUpgradeCommand { get; }
    public RelayCommand<object> RemoveQualificationUpgradeCommand { get; }
    public RelayCommand AddRetrainingCommand { get; }
    public RelayCommand<object> RemoveRetrainingCommand { get; }
    public RelayCommand AddEducationCommand { get; }
    public RelayCommand<object> RemoveEducationCommand { get; }
    public RelayCommand AddAppointmentCommand { get; }
    public RelayCommand<object> RemoveAppointmentCommand { get; }

    private async Task SaveAsync()
    {
        ValidateAll();
        if (HasErrors) return;

        if (_originalEmployee != null)
        {
            var result = MessageBox.Show("Сохранить изменения в карточке сотрудника?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
        }

        IsSaving = true;
        try
        {
            var employee = new Employee
            {
                Id = _originalEmployee?.Id ?? 0,
                LastName = LastName.Trim(),
                FirstName = FirstName.Trim(),
                MiddleName = string.IsNullOrWhiteSpace(MiddleName) ? null : MiddleName.Trim(),
                IsMale = IsMale,
                BirthDate = BirthDate,
                BirthPlace = string.IsNullOrWhiteSpace(BirthPlace) ? null : BirthPlace.Trim(),
                IsUnionMember = IsUnionMember,
                Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
                Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                HomePhone = string.IsNullOrWhiteSpace(HomePhone) ? null : HomePhone.Trim(),
                DiplomaSpecialty = string.IsNullOrWhiteSpace(DiplomaSpecialty) ? null : DiplomaSpecialty.Trim(),
                DiplomaQualification = string.IsNullOrWhiteSpace(DiplomaQualification) ? null : DiplomaQualification.Trim(),
                MainProfession = string.IsNullOrWhiteSpace(MainProfession) ? null : MainProfession.Trim(),
                SpecialtyExperienceYears = SpecialtyExperienceYears,
                TotalExperienceYears = TotalExperienceYears,
                HireDate = HireDate,
                LastWorkPlace = string.IsNullOrWhiteSpace(LastWorkPlace) ? null : LastWorkPlace.Trim(),
                LastWorkPosition = string.IsNullOrWhiteSpace(LastWorkPosition) ? null : LastWorkPosition.Trim(),
                LastWorkDismissalDate = LastWorkDismissalDate,
                LastWorkDismissalReason = string.IsNullOrWhiteSpace(LastWorkDismissalReason) ? null : LastWorkDismissalReason.Trim(),
                Category = string.IsNullOrWhiteSpace(Category) ? null : Category.Trim(),
                CurrentProfession = string.IsNullOrWhiteSpace(CurrentPosition) ? null : CurrentPosition.Trim(),
                ContractEndDate = ContractEndDate,
                AdditionalInfo = string.IsNullOrWhiteSpace(AdditionalInfo) ? null : AdditionalInfo.Trim(),

                Passport = BuildOrUpdatePassport(),
                MilitaryRegistration = BuildMilitaryOrNull(),
                EducationRecords = EducationItems.ToList(),
                AppointmentTransfers = AppointmentItems.ToList(),
                QualificationUpgrades = QualificationUpgradeItems.ToList(),
                RetrainingRecords = RetrainingItems.ToList(),
                AttestationRecords = AttestationItems.ToList(),
                FamilyMembers = FamilyMembers.ToList(),
                VacationRecords = VacationItems.ToList()
            };

            Employee saved = _originalEmployee != null
                ? await _employeesService.UpdateAsync(employee)
                : await _employeesService.AddAsync(employee);

            _onSaved(saved);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private Passport? BuildOrUpdatePassport()
    {
        if (string.IsNullOrWhiteSpace(PassportNumber))
            return null;

        return new Passport
        {
            EmployeeId = _originalEmployee?.Id ?? 0, 

            Series = string.IsNullOrWhiteSpace(PassportSeries)
                ? null
                : PassportSeries.Trim(),

            Number = PassportNumber.Trim(),

            IdentificationNumber = string.IsNullOrWhiteSpace(IdentificationNumber)
                ? null
                : IdentificationNumber.Trim(),

            IssuedBy = string.IsNullOrWhiteSpace(IssuedBy)
                ? null
                : IssuedBy.Trim(),

            IssueDate = IssueDate,
            ExpiryDate = PassportExpiryDate         
        };
    }

    private MilitaryRegistration? BuildMilitaryOrNull()
    {
        if (string.IsNullOrWhiteSpace(MilRank)) return null;
        return new MilitaryRegistration
        {
            AccountingGroup = string.IsNullOrWhiteSpace(MilAccountingGroup) ? null : MilAccountingGroup.Trim(),
            AccountingCategory = string.IsNullOrWhiteSpace(MilAccountingCategory) ? null : MilAccountingCategory.Trim(),
            Composition = string.IsNullOrWhiteSpace(MilComposition) ? null : MilComposition.Trim(),
            MilitaryRank = MilRank.Trim(),
            MilitarySpecialty = string.IsNullOrWhiteSpace(MilSpecialty) ? null : MilSpecialty.Trim(),
            FitnessCategory = string.IsNullOrWhiteSpace(MilFitnessCategory) ? null : MilFitnessCategory.Trim(),
            CommissariatName = string.IsNullOrWhiteSpace(MilCommissariatName) ? null : MilCommissariatName.Trim(),
            SpecialAccountingNumber = string.IsNullOrWhiteSpace(MilSpecialNumber) ? null : MilSpecialNumber.Trim()
        };
    }

    private void ValidateAll()
    {
        ValidateLastName();
        ValidateFirstName();
        ValidateMiddleName();
        ValidateBirthDate();
        ValidateBirthPlace();
        ValidateAddress();
        ValidatePhone();
        ValidateHomePhone();
        ValidateCurrentPosition();
        ValidatePassportSeries();
        ValidatePassportNumber();
        ValidateIdentificationNumber();
        ValidateIssuedBy();
        ValidatePassportExpiryDate();
    }

    private void ValidateLastName() { ClearErrors(nameof(LastName)); if (string.IsNullOrWhiteSpace(LastName)) AddError(nameof(LastName), "Введите фамилию."); }
    private void ValidateFirstName() { ClearErrors(nameof(FirstName)); if (string.IsNullOrWhiteSpace(FirstName)) AddError(nameof(FirstName), "Введите имя."); }
    private void ValidateMiddleName() { ClearErrors(nameof(MiddleName)); if (MiddleName.Length > 64) AddError(nameof(MiddleName), "Отчество слишком длинное."); }
    private void ValidateBirthDate() { ClearErrors(nameof(BirthDate)); if (BirthDate > DateTime.Today.AddYears(-14)) AddError(nameof(BirthDate), "Работник должен быть старше 14 лет."); }
    private void ValidateBirthPlace() { ClearErrors(nameof(BirthPlace)); if (BirthPlace.Length > 256) AddError(nameof(BirthPlace), "Место рождения слишком длинное."); }
    private void ValidateAddress() { ClearErrors(nameof(Address)); if (Address.Length > 256) AddError(nameof(Address), "Адрес слишком длинный."); }
    private static readonly Regex PhoneRegex = new(@"^[0-9+\-() ]*$", RegexOptions.Compiled);
    private void ValidatePhone() { ClearErrors(nameof(Phone)); if (!string.IsNullOrEmpty(Phone) && !PhoneRegex.IsMatch(Phone)) AddError(nameof(Phone), "Телефон содержит недопустимые символы."); }
    private void ValidateHomePhone() { ClearErrors(nameof(HomePhone)); if (!string.IsNullOrEmpty(HomePhone) && !PhoneRegex.IsMatch(HomePhone)) AddError(nameof(HomePhone), "Телефон содержит недопустимые символы."); }
    private void ValidateCurrentPosition() { ClearErrors(nameof(CurrentPosition)); if (CurrentPosition.Length > 128) AddError(nameof(CurrentPosition), "Должность слишком длинная."); }
    private void ValidatePassportSeries() { ClearErrors(nameof(PassportSeries)); if (PassportSeries.Length > 16) AddError(nameof(PassportSeries), "Серия слишком длинная."); }
    private void ValidatePassportNumber() { ClearErrors(nameof(PassportNumber)); if (PassportNumber.Length > 16) AddError(nameof(PassportNumber), "Номер слишком длинный."); }
    private void ValidateIdentificationNumber() { ClearErrors(nameof(IdentificationNumber)); if (IdentificationNumber.Length > 32) AddError(nameof(IdentificationNumber), "Идентификационный номер слишком длинный."); }
    private void ValidateIssuedBy() { ClearErrors(nameof(IssuedBy)); if (IssuedBy.Length > 256) AddError(nameof(IssuedBy), "Поле 'Кем выдан' слишком длинное."); }

    public ObservableCollection<FamilyMember> FamilyMembers => _familyMembers;
    public ObservableCollection<AttestationRecord> AttestationItems => _attestationItems;
    public ObservableCollection<QualificationUpgrade> QualificationUpgradeItems => _qualificationUpgradeItems;
    public ObservableCollection<RetrainingRecord> RetrainingItems => _retrainingItems;
    public ObservableCollection<AppointmentTransfer> AppointmentItems => _appointmentItems;
    public ObservableCollection<EducationRecord> EducationItems => _educationItems;
    public ObservableCollection<VacationRecord> VacationItems => _vacationItems;
    public ObservableCollection<RelationType> RelationTypes => _relationTypes;
    public ObservableCollection<EducationLevel> EducationLevels => _educationLevels;


    private void LoadExistingEmployee(Employee emp)
    {

        LastName = emp.LastName ?? "";
        FirstName = emp.FirstName ?? "";
        MiddleName = emp.MiddleName ?? "";
        IsMale = emp.IsMale;                    
        BirthDate = emp.BirthDate;
        BirthPlace = emp.BirthPlace ?? "";
        IsUnionMember = emp.IsUnionMember;
        Address = emp.Address ?? "";
        Phone = emp.Phone ?? "";
        HomePhone = emp.HomePhone ?? "";
        AdditionalInfo = emp.AdditionalInfo ?? "";
        CurrentPosition = emp.CurrentProfession ?? "";
        DiplomaSpecialty = emp.DiplomaSpecialty ?? "";
        DiplomaQualification = emp.DiplomaQualification ?? "";
        MainProfession = emp.MainProfession ?? "";
        Category = emp.Category ?? "";   

        SpecialtyExperienceYears = emp.SpecialtyExperienceYears;
        TotalExperienceYears = emp.TotalExperienceYears;
        HireDate = emp.HireDate;
        LastWorkPlace = emp.LastWorkPlace ?? "";
        LastWorkPosition = emp.LastWorkPosition ?? "";
        LastWorkDismissalDate = emp.LastWorkDismissalDate;
        LastWorkDismissalReason = emp.LastWorkDismissalReason ?? "";
        ContractEndDate = emp.ContractEndDate;

        if (emp.Passport != null)
        {
            PassportSeries = emp.Passport.Series ?? "";
            PassportNumber = emp.Passport.Number ?? "";
            IdentificationNumber = emp.Passport.IdentificationNumber ?? "";
            IssuedBy = emp.Passport.IssuedBy ?? "";
            IssueDate = emp.Passport.IssueDate;
            PassportExpiryDate = emp.Passport.ExpiryDate;
        }

        if (emp.MilitaryRegistration != null)
        {
            MilAccountingGroup = emp.MilitaryRegistration.AccountingGroup ?? "";
            MilAccountingCategory = emp.MilitaryRegistration.AccountingCategory ?? "";
            MilComposition = emp.MilitaryRegistration.Composition ?? "";
            MilRank = emp.MilitaryRegistration.MilitaryRank ?? "";
            MilSpecialty = emp.MilitaryRegistration.MilitarySpecialty ?? "";
            MilFitnessCategory = emp.MilitaryRegistration.FitnessCategory ?? "";
            MilCommissariatName = emp.MilitaryRegistration.CommissariatName ?? "";
            MilSpecialNumber = emp.MilitaryRegistration.SpecialAccountingNumber ?? "";
        }

        _educationItems.Clear();

        foreach (var record in emp.EducationRecords ?? Enumerable.Empty<EducationRecord>())
        {

            if (record.EducationLevel != null)
            {
                var matching = _educationLevels.FirstOrDefault(l => l.Id == record.EducationLevel.Id);
                if (matching != null)
                {
                    record.EducationLevel = matching;
                }
            }
            else if (!string.IsNullOrWhiteSpace(record.CustomEducationLevel))
            {
                var matching = _educationLevels.FirstOrDefault(l =>
                    l.Name.Equals(record.CustomEducationLevel.Trim(), StringComparison.OrdinalIgnoreCase));

                if (matching != null)
                {
                    record.EducationLevel = matching;
                }
            }

            record.PropertyChanged += (_, _) => RaiseEducationSectionStatus();
            _educationItems.Add(record);
        }

        _vacationItems.Clear();
        foreach (var v in emp.VacationRecords ?? Enumerable.Empty<VacationRecord>())
        {
            v.PropertyChanged += (_, _) => RaiseVacationSectionStatus();
            _vacationItems.Add(v);
        }
        _familyMembers.Clear();
        foreach (var f in emp.FamilyMembers ?? Enumerable.Empty<FamilyMember>()) _familyMembers.Add(f);

        _qualificationUpgradeItems.Clear();
        foreach (var q in emp.QualificationUpgrades ?? Enumerable.Empty<QualificationUpgrade>()) _qualificationUpgradeItems.Add(q);

        _retrainingItems.Clear();
        foreach (var r in emp.RetrainingRecords ?? Enumerable.Empty<RetrainingRecord>()) _retrainingItems.Add(r);

        _attestationItems.Clear();
        foreach (var a in emp.AttestationRecords ?? Enumerable.Empty<AttestationRecord>()) _attestationItems.Add(a);

        _appointmentItems.Clear();
        foreach (var a in emp.AppointmentTransfers ?? Enumerable.Empty<AppointmentTransfer>())
        {
            a.PropertyChanged += (_, _) => RaiseAppointmentSectionStatus();
            _appointmentItems.Add(a);
        }

        RaiseAllSectionStatuses();

    }

}