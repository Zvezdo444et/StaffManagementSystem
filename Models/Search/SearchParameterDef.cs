namespace Inspector.Models.Search;

public sealed record SearchParameterDef(
    string Key,
    string Category,
    string DisplayName,
    SearchFilterType FilterType);
