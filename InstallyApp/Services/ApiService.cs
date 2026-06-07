using System.Net.Http;
using System.Net.Http.Json;
using InstallyAPI.Models;

namespace InstallyApp.DataServices;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:23842")
        };
    }

    public async Task<List<UserEntity>> GetUsers()
    {
        return await _http.GetFromJsonAsync<List<UserEntity>>("api/user")
               ?? new List<UserEntity>();
    }

    public async Task<List<PackageEntity>> GetPackages()
    {
        return await _http.GetFromJsonAsync<List<PackageEntity>>("api/package")
               ?? new List<PackageEntity>();
    }

    public async Task<List<CollectionEntity>> GetCollections()
    {
        return await _http.GetFromJsonAsync<List<CollectionEntity>>("api/collection")
               ?? new List<CollectionEntity>();
    }
}