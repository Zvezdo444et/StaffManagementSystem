using System.Collections.ObjectModel;
using System.Windows.Input;
using Inspector.Services;

namespace Inspector.ViewModels;

public sealed class NotificationsViewModel : ObservableObject
{
    private readonly IEmployeesService _employeesService;
    public ObservableCollection<Notification> Notifications { get; } = new();

    public ICommand DismissCommand { get; }
    public ICommand RefreshCommand { get; }

    private int _unreadCount;
    public int UnreadCount
    {
        get => _unreadCount;
        private set => SetProperty(ref _unreadCount, value);
    }

    public bool HasNotifications => UnreadCount > 0;
    public bool HasNoNotifications => UnreadCount == 0;

    public NotificationsViewModel(IEmployeesService employeesService)
    {
        _employeesService = employeesService;
        DismissCommand = new RelayCommand<Notification>(DismissNotification);
        RefreshCommand = new RelayCommand(async () => await LoadNotifications());
        _ = LoadNotifications();
    }

    private async Task LoadNotifications()
    {
        Notifications.Clear();

        var employees = await _employeesService.GetAllAsync();

        foreach (var emp in employees)
        {
            if (emp.Passport?.ExpiryDate.HasValue == true)
            {
                var daysLeft = (emp.Passport.ExpiryDate.Value - DateTime.Today).Days;
                if (daysLeft <= 7 && daysLeft >= 0)
                {
                    Notifications.Add(new Notification
                    {
                        Id = Guid.NewGuid(),
                        Title = "Паспорт",
                        Message = $"{emp.FullName} — паспорт истекает через {daysLeft} дней ({emp.Passport.ExpiryDate.Value:dd.MM.yyyy})",
                        Type = "Passport",
                        RelatedId = emp.Id
                    });
                }
            }
        }

        UpdateCounts();
    }

    private void DismissNotification(Notification notification)
    {
        if (notification == null) return;
        Notifications.Remove(notification);
        UpdateCounts();
    }

    private void UpdateCounts()
    {
        UnreadCount = Notifications.Count;
        OnPropertyChanged(nameof(HasNoNotifications));
        OnPropertyChanged(nameof(HasNotifications));
    }
    public async Task CheckAndShowStartupNotifications()
    {
        await LoadNotifications();
        if (Notifications.Count > 0)
        {
            var text = string.Join("\n\n", Notifications.Select(n => $"• {n.Title}: {n.Message}"));
            System.Windows.MessageBox.Show(text, "Уведомления", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }
}

public sealed class Notification
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int? RelatedId { get; set; }
}