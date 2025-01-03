using System.IO;
using System.Net.Http;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using InstallyAPI.Commands.PackageCommands;
using InstallyAPI.Models;
using InstallyAPI.Queries.Interfaces;
using InstallyApp.Utils.Functions;
using Microsoft.Extensions.DependencyInjection;

namespace InstallyApp.DataServices;

public static class GetPackages
{
    public static List<PackageEntity> Packages { get; set; } = new List<PackageEntity>();

    public static async Task<bool> LoadAPIPackages()
{
    // Load local packages
    var localDataPkgs = App.Services.GetService<IPackageQuery>().GetAll().ToList();
    Packages = localDataPkgs;

    if (localDataPkgs.Count < 4000)
    {
        try
        {
            string apiPkgs = await API.Get("?limit=4315").ConfigureAwait(false);

            List<PackageEntity> apiPackageEntities = Json.JsonToClassPkg(apiPkgs);

            foreach (var apiPackage in apiPackageEntities)
            {
                // Check if the package already exists in local data by its GUID
                var existingPackage = localDataPkgs.FirstOrDefault(p => p.Guid == apiPackage.Guid);

                if (existingPackage != null)
                {
                    existingPackage.WingetId = apiPackage.WingetId;
                    existingPackage.Name = apiPackage.Name;
                    existingPackage.Publisher = apiPackage.Publisher;
                    existingPackage.TagsString = apiPackage.TagsString;
                    existingPackage.Description = apiPackage.Description;
                    existingPackage.Site = apiPackage.Site;
                    existingPackage.VersionsLength = apiPackage.VersionsLength;
                    existingPackage.LatestVersion = apiPackage.LatestVersion;
                    existingPackage.Score = apiPackage.Score;
                }
                else
                {
                    // Add new package with its GUID
                    Packages.Add(apiPackage);
                    
                    App.Packages = Packages;
                }
            }

            AddPackagesCommand command = new(Packages);
            bool result = await App.Mediator.Send(command).ConfigureAwait(false);

            if (result)
            {
                Packages = Packages.OrderBy(p => p.Name).ToList();
            }

            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading packages: {ex.Message}");
            return false;
        }
    }
    return true;
}


    public static List<PackageEntity> CatchPackages(string? namePart, string? category, int offset, int limit)
    {
        string searchByName = string.Empty;

        if (namePart is not null) searchByName = namePart.ToLower();
        
        IQueryable<PackageEntity> apiPkgsQuery = App.Services
            .GetService<IPackageQuery>()
            .GetAll();
            
        bool FilterByName(PackageEntity pkg)
        {
            bool apiPkgsQuery = pkg.Name.ToLower().Contains(searchByName.ToLower());
            
            return apiPkgsQuery;
        }
        
        bool FilterByCategory(PackageEntity pkg)
        {
            bool pkgFilteredByName = pkg.Name.ToLower().Contains(searchByName.ToLower());

            if (!string.IsNullOrEmpty(category))
            {
                if (App.Main.AppSearchWindow.CategoriesDict.ContainsKey(category))
                {
                    var categoryTags = App.Main.AppSearchWindow.CategoriesDict[category];

                    bool pkgFilteredByCategory = pkg.Tags.Any(tag => categoryTags.Contains(tag, StringComparer.OrdinalIgnoreCase));

                    return pkgFilteredByName && pkgFilteredByCategory;
                }
                else
                {
                    return pkgFilteredByName;
                }
            }

            return pkgFilteredByName;
        }

        // Applying filters
        List<InstallyAPI.Models.PackageEntity> apiPkgs = apiPkgsQuery.AsEnumerable()
            .Where(pkg => FilterByName(pkg))
            .Where(pkg => FilterByCategory(pkg))
            .OrderByDescending(pkg => pkg.Score)
            .Skip(offset)
            .Take(limit)
            .ToList();

        return apiPkgs;
    }
    
    public static PackageEntity CatchPackages(Guid pkgGuid)
    {
        PackageEntity pkg = Packages.Find(pkg => pkg.Guid == pkgGuid);
        
        return pkg;
    }

    public static async Task<Control?> CatchPackagesFavicon(Guid pkgGuid)
    {
        if (Packages is null) return null;

        var dictionary = Packages.ToDictionary(x => x.Guid);
        PackageEntity pkg = dictionary.ContainsKey(pkgGuid) ? dictionary[pkgGuid] : null;
        
        if (pkg is null) return null;

        if (pkg.Site is null)
        {
            TextBlock textBlock = new TextBlock()
            {
                Text = pkg.Name[0].ToString().ToUpper(),
                Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 30,
            };

            return textBlock;
        }
        else
        {
            var urlDoFavicon = $"{API.SiteUrl}/icons/{pkg.WingetId}.png";

            Image image = new Image()
            {
                Name = "IconImage",
                Height = 30,
                Width = 30,
            };

            Bitmap bitmap = await LoadBitmapFromUrlAsync(urlDoFavicon);
            
            image.Source = bitmap;

            return image;
        }
    }
    
    public static async Task<Bitmap> LoadBitmapFromUrlAsync(string url)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                // Get the image data as a stream
                await using var imageStream = await response.Content.ReadAsStreamAsync();
                return new Bitmap(imageStream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }
}