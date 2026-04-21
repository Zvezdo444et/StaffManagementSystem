using Inspector.Services;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Inspector.ViewModels;

public sealed class ReportsViewModel : ObservableObject
{
    private readonly IStatisticsService _statistics;
    private readonly IExportService _exportService;
    private readonly IEmployeesService _employeesService;

    private int _totalEmployees;
    private int _maleEmployees;
    private int _femaleEmployees;
    private int _onVacationEmployees;
    private int _pensionAgeEmployees;
    private int _averageAge;
    public ICommand ExportGeneralListCommand { get; }
    public ICommand ExportContractListCommand { get; }
    public ICommand ExportVacationListCommand { get; }
    public ICommand ExportCategoryListCommand { get; }
    public ReportsViewModel(
    IStatisticsService statistics,
    IExportService exportService,
    IEmployeesService employeesService)
    {
        _statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _employeesService = employeesService ?? throw new ArgumentNullException(nameof(employeesService));

        ExportGeneralListCommand = new AsyncRelayCommand(ExportGeneralListAsync);
        ExportContractListCommand = new AsyncRelayCommand(ExportContractListAsync);
        ExportVacationListCommand = new AsyncRelayCommand(ExportVacationListAsync);
        ExportCategoryListCommand = new AsyncRelayCommand(ExportCategoryListAsync);
        _ = LoadAsync();
    }

    public string Title => "Отчёты";

    public int TotalEmployees { get => _totalEmployees; private set => SetProperty(ref _totalEmployees, value); }
    public int MaleEmployees { get => _maleEmployees; private set => SetProperty(ref _maleEmployees, value); }
    public int FemaleEmployees { get => _femaleEmployees; private set => SetProperty(ref _femaleEmployees, value); }
    public int OnVacationEmployees { get => _onVacationEmployees; private set => SetProperty(ref _onVacationEmployees, value); }
    public int PensionAgeEmployees { get => _pensionAgeEmployees; private set => SetProperty(ref _pensionAgeEmployees, value); }
    public int AverageAge { get => _averageAge; private set => SetProperty(ref _averageAge, value); }

    private async Task LoadAsync()
    {
        try
        {
            var stats = await _statistics.GetEmployeeStatisticsAsync();
            TotalEmployees = stats.TotalEmployees;
            MaleEmployees = stats.MaleEmployees;
            FemaleEmployees = stats.FemaleEmployees;
            OnVacationEmployees = stats.OnVacationEmployees;
            PensionAgeEmployees = stats.PensionAgeEmployees;
            AverageAge = stats.AverageAge;
        }
        catch { /* оставляем нули */ }
    }

    private async Task ExportGeneralListAsync()
    {
        var dialog = new SaveFileDialog
        {
            FileName = "Список_общий",
            DefaultExt = ".xlsx",
            Filter = "Файлы Excel (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*",
            Title = "Сохранить общий список работников",
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

            var employees = await _employeesService.GetAllAsync();

            await _exportService.ExportEmployeesToExcelAsync(employees, dialog.FileName);

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
            MessageBox.Show($"Ошибка при сохранении отчёта:\n{ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private async Task ExportCategoryListAsync()
    {
        var dialog = new SaveFileDialog
        {
            FileName = "Категории_работников",
            DefaultExt = ".xlsx",
            Filter = "Файлы Excel (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*",
            Title = "Сохранить список категорий работников",
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

            var employees = await _employeesService.GetAllAsync();

            await _exportService.ExportCategoryesToExcelAsync(employees, dialog.FileName);

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
            MessageBox.Show($"Ошибка при сохранении отчёта:\n{ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ExportVacationListAsync()
    {
        var dialog = new SaveFileDialog
        {
            FileName = "Список_отпусков",
            DefaultExt = ".xlsx",
            Filter = "Файлы Excel (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*",
            Title = "Сохранить список отпусков работников",
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

            var employees = await _employeesService.GetAllAsync();

            await _exportService.ExportVacationsToExcelAsync(employees, dialog.FileName);

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
            MessageBox.Show($"Ошибка при сохранении отчёта:\n{ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private async Task ExportContractListAsync()
    {
        var dialog = new SaveFileDialog
        {
            FileName = "Контракты_работников",
            DefaultExt = ".xlsx",
            Filter = "Файлы Excel (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*",
            Title = "Сохранить список контрактов работников",
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

            var employees = await _employeesService.GetAllAsync();

            await _exportService.ExportContractsToExcelAsync(employees, dialog.FileName);

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
            MessageBox.Show($"Ошибка при сохранении отчёта:\n{ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}