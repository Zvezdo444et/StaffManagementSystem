namespace Inspector.Models.Search;

public sealed class SearchTemplate
{
    public List<string> SelectedKeys { get; set; } = new();
    public Dictionary<string, string> FilterValues { get; set; } = new();
}
