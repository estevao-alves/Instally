using InstallyAPI.Data;
using InstallyApp;
using InstallyApp.DataServices;

public class SyncService
{
    private readonly ApplicationDbContext _context;
    private readonly IRestDataService _api;

    public SyncService(ApplicationDbContext context, IRestDataService api)
    {
        _context = context;
        _api = api;
    }

    public async Task SyncAll()
    {
        await SyncPackages();
        await SyncCollections();
    }

    public async Task SyncPackages()
    {
        var packages = await _api.GetAllPackagesAsync(); // ✅ HERE

        if (!packages.Any()) return;

        _context.Packages.RemoveRange(_context.Packages);
        await _context.Packages.AddRangeAsync(packages);
        await _context.SaveChangesAsync();
        App.Packages = packages;
    }

    public async Task SyncCollections()
    {
        var collections = await _api.GetAllCollectionsAsync(); // ✅ HERE

        if (!collections.Any()) return;

        _context.Collections.RemoveRange(_context.Collections);
        await _context.Collections.AddRangeAsync(collections);
        await _context.SaveChangesAsync();
        App.Collections = collections;
    }
}