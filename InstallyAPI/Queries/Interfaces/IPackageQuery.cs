using InstallyAPI.Models;

namespace InstallyAPI.Queries.Interfaces
{
    public interface IPackageQuery
    {
        IQueryable<PackageEntity> GetAll();
        Task<PackageEntity> GetById(Guid id);
    }
}
