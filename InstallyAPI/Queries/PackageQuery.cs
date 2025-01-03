using InstallyAPI.Data;
using InstallyAPI.Models;
using InstallyAPI.Queries.Interfaces;

namespace InstallyAPI.Queries
{
    public class PackageQuery : IPackageQuery
    {
        private readonly ApplicationDbContext _appRepository;

        public PackageQuery(ApplicationDbContext appRepository)
        {
            _appRepository = appRepository;
        }
        public IQueryable<PackageEntity> GetAll()
        {
            return _appRepository.Packages.AsQueryable();
        }

        public async Task<PackageEntity> GetById(Guid id)
        {
            return await _appRepository.Set<PackageEntity>().FindAsync(id);
        }
    }
}
