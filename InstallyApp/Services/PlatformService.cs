using InstallyAPI.Models;

namespace InstallyApp.DataServices;
using System.Runtime.InteropServices;

public static class PlatformService
{
    public static string PackageSource { get; private set; }

    public static void Initialize()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            PackageSource = "Winget";

        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            PackageSource = "Flatpak";

        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            PackageSource = "Brew";
    }
    
    public static bool IsPackageSupported(PackageEntity pkg)
    {
        if (pkg?.PackageIds == null)
            return false;

        var source = PackageSource;

        return pkg.PackageIds.TryGetValue(source, out var id) &&
               !string.IsNullOrEmpty(id);
    }
}