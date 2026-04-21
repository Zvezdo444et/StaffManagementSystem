using System.ComponentModel;
using Inspector.Models.Search;

namespace Inspector.ViewModels.Search;

public sealed class SearchParameterViewModel : INotifyPropertyChanged
{
    private bool _isSelected;
    private string _filterValue = string.Empty;

    public SearchParameterViewModel(SearchParameterDef def)
    {
        Key = def.Key;
        Category = def.Category;
        DisplayName = def.DisplayName;
        FilterType = def.FilterType;
    }

    public string Key { get; }
    public string Category { get; }
    public string DisplayName { get; }
    public SearchFilterType FilterType { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged(nameof(IsSelected));
        }
    }

    public string FilterValue
    {
        get => _filterValue;
        set
        {
            if (_filterValue == value) return;
            _filterValue = value ?? string.Empty;
            OnPropertyChanged(nameof(FilterValue));
        }
    }

    public void Reset()
    {
        IsSelected = false;
        FilterValue = string.Empty;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
