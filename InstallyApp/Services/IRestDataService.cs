using InstallyAPI.Models;
using InstallyApp.Models;

namespace InstallyApp.DataServices;

public interface IRestDataService
{
    Task<List<UserEntity>> GetAllUsersAsync();
    Task AddUserAsync(UserEntity toDo);
    Task UpdateUserAsync(UserEntity toDo);
    Task DeleteUserAsync(Guid id);
    
    Task<List<PackageEntity>> GetAllPackagesAsync();
    Task<List<CollectionEntity>> GetAllCollectionsAsync();
}