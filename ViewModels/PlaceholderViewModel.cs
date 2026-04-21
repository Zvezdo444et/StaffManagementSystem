namespace Inspector.ViewModels;
public sealed class PlaceholderViewModel : ObservableObject
{
    public PlaceholderViewModel(string title)
    {
        Title = title;
    }
    public string Title { get; }
}

