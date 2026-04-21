using Inspector.Services;
using Inspector.ViewModels;
using Inspector.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace Inspector
{
    public partial class App : Application
    {
        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level =
                System.Diagnostics.SourceLevels.Critical;

            DispatcherUnhandledException += (_, args) =>
            {
                MessageBox.Show(
                    args.Exception.ToString(),
                    "Ошибка приложения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                args.Handled = true;
            };

            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory);
                    config.AddJsonFile("connection.json", optional: false, reloadOnChange: false);
                })
                .ConfigureServices((context, services) =>
                {
                    var connectionString = context.Configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException(
                            "Строка подключения не задана. Создайте файл connection.json с ключом ConnectionStrings:DefaultConnection.");

                    services.AddDbContextFactory<Inspector.Data.InspectorDbContext>(options =>
                        options.UseSqlServer(connectionString));

                    services.AddScoped<IEmployeesService, EmployeesService>();
                    services.AddScoped<IEmployeeSearchService, EmployeeSearchService>();
                    services.AddScoped<IAuthService, AuthService>();
                    services.AddScoped<IStatisticsService, StatisticsService>();
                    services.AddScoped<IPensionAgeSettingsService, PensionAgeSettingsService>();
                    services.AddScoped<IExportService, ExportService>();

                    services.AddSingleton<NavigationStore>();
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<NotificationsViewModel>();

                    services.AddTransient<LoginWindow>(sp =>
                    {
                        var authService = sp.GetRequiredService<IAuthService>();
                        var mainVm = sp.GetRequiredService<MainViewModel>();

                        return new LoginWindow(authService, successfulUser =>
                        {
                            mainVm.CurrentUser = successfulUser;

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var mainWindow = new MainWindow(mainVm);
                                mainWindow.Show();
                            });

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (Application.Current.MainWindow is LoginWindow loginWnd)
                                    loginWnd.DialogResult = true;
                            });
                        });
                    });

                    services.AddTransient<SettingsViewModel>(sp =>
                    {
                        var employeesService = sp.GetRequiredService<IEmployeesService>();
                        var authService = sp.GetRequiredService<IAuthService>();
                        var mainVm = sp.GetRequiredService<MainViewModel>();

                        if (mainVm.CurrentUser == null)
                            throw new InvalidOperationException("CurrentUser не установлен при создании SettingsViewModel");

                        return new SettingsViewModel(employeesService, authService, mainVm.CurrentUser);
                    });

                    services.AddTransient<Inspector.ViewModels.Employees.EmployeesListViewModel>();
                    services.AddSingleton<Func<Inspector.ViewModels.Employees.EmployeesListViewModel>>(sp =>
                        () => sp.GetRequiredService<Inspector.ViewModels.Employees.EmployeesListViewModel>());

                    services.AddTransient<Inspector.ViewModels.ReportsViewModel>();
                    services.AddSingleton<Func<Inspector.ViewModels.ReportsViewModel>>(sp =>
                        () => sp.GetRequiredService<Inspector.ViewModels.ReportsViewModel>());

                    services.AddTransient<Inspector.ViewModels.Search.SearchViewModel>();
                    services.AddSingleton<Func<Inspector.ViewModels.Search.SearchViewModel>>(sp =>
                        () => sp.GetRequiredService<Inspector.ViewModels.Search.SearchViewModel>());
                })
                .Build();

            _host.Start();

            var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
            bool? result = loginWindow.ShowDialog();

            if (result != true)
            {
                _host.StopAsync().Wait();
                Shutdown();
                return;
            }

            _ = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    using var scope = _host.Services.CreateScope();
                    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<Inspector.Data.InspectorDbContext>>();
                    using var db = factory.CreateDbContext();
                    db.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(
                            "Не удалось подключиться к базе данных.\n\n" +
                            "Проверьте SQL Server и файл connection.json.\n\n" + ex.Message,
                            "Ошибка подключения к БД",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    });
                }
            });

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host is not null)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
                _host.Dispose();
            }
            base.OnExit(e);
        }
    }
}