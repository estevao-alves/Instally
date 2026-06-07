namespace InstallyApp.Services;

public static class SelectionService
{
    private static readonly HashSet<Guid> _selectedPackages = new();

    public static event Action<Guid>? SelectionChanged;

    public static bool IsSelected(Guid pkgId)
        => _selectedPackages.Contains(pkgId);

    public static void Select(Guid pkgId)
    {
        if (_selectedPackages.Add(pkgId))
            SelectionChanged?.Invoke(pkgId);
    }

    public static void Deselect(Guid pkgId)
    {
        if (_selectedPackages.Remove(pkgId))
            SelectionChanged?.Invoke(pkgId);
    }

    public static void Toggle(Guid pkgId)
    {
        if (!_selectedPackages.Add(pkgId))
            _selectedPackages.Remove(pkgId);

        SelectionChanged?.Invoke(pkgId);
    }

    public static IReadOnlyCollection<Guid> GetAll()
        => _selectedPackages;

    public static void Clear()
    {
        foreach (var pkgId in _selectedPackages.ToList())
        {
            SelectionChanged?.Invoke(pkgId);
        }

        _selectedPackages.Clear();
    }
}