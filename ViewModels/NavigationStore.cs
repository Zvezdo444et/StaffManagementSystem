namespace Inspector.ViewModels;

public sealed class NavigationStore
{
    private ObservableObject? _currentViewModel;

    public ObservableObject? CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            if (ReferenceEquals(_currentViewModel, value))
                return;

            _currentViewModel = value;
            CurrentViewModelChanged?.Invoke();
        }
    }

    public event Action? CurrentViewModelChanged;
}

