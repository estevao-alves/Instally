using System.Net.Http;

namespace InstallyApp.DataServices;

public static class API
{
    public static string SiteUrl = "https://instally.app";
    
    // DEV ONLY, Remove in production -----------
    // public static string SiteUrl = "https://dev.instally.app";
    // ------
    
    public static string BaseUrl = $"{SiteUrl}/api";

    public static async Task<string> Get(string pathname)
    {
        try
        {
            var client = new HttpClient();
            using HttpResponseMessage response = await client.GetAsync(BaseUrl + pathname);
            
            response.EnsureSuccessStatusCode();
                
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception: {ex.Message}");
            Debug.WriteLine(BaseUrl + pathname);
            return null;
        }
    }
}