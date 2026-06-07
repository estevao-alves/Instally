using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
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
    
    public static PackageEntity CatchPackage(Guid pkgGuid)
    {
        PackageEntity pkg = App.Packages.Find(pkg => pkg.Guid == pkgGuid);
        
        return pkg;
    }

    public static async Task<Control?> CatchPackagesFavicon(Guid pkgGuid)
    {
        if (App.Packages is null) return null;

        var pkg = App.Packages.FirstOrDefault(p => p.Guid == pkgGuid);
        
        if (pkg is null) return null;
        
        Image image = new Image()
        {
            Name = "IconImage",
            Height = 30,
            Width = 30,
        };
        
        string? packageId = null;

        Bitmap? bitmap = null;

        // Hosted icons first
        pkg.PackageIds?.TryGetValue("Winget", out packageId);

        if (!string.IsNullOrEmpty(packageId))
        {
            string faviconUrl = $"{API.SiteUrl}/icons/{packageId}.png";

            bitmap = await LoadBitmapFromUrlAsync(faviconUrl);
        }

        // Fallback to Flatpak icon
        if (bitmap == null && !string.IsNullOrEmpty(pkg.Icon))
        {
            bitmap = await LoadBitmapFromUrlAsync(pkg.Icon);
        }

        // Fallback to Google favicon
        if (bitmap == null && !string.IsNullOrEmpty(pkg.Site))
        {
            string faviconUrl =
                $"https://www.google.com/s2/favicons?domain={pkg.Site}&sz=256";

            bitmap = await LoadBitmapFromUrlAsync(faviconUrl);
        }
        
        // Fallback to Name Letter
        if (bitmap == null)
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

        image.Source = bitmap;

        return image;
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