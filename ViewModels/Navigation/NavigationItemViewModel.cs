namespace Inspector.ViewModels.Navigation;

public sealed class NavigationItemViewModel : ObservableObject
{
    private bool _isActive;
    private int _badgeCount;

    public NavigationItemViewModel(string title, RelayCommand command)
    {
        Title = title;
        Command = command;
    }

    public string Title { get; }

    public RelayCommand Command { get; }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public int BadgeCount
    {
        get => _badgeCount;
        set => SetProperty(ref _badgeCount, value);
    }
}