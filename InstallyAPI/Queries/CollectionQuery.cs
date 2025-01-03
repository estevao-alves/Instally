using InstallyAPI.Data;
using InstallyAPI.Models;
using InstallyAPI.Queries.Interfaces;

namespace InstallyAPI.Queries
{
    public class CollectionQuery : ICollectionQuery
    {
        private readonly ApplicationDbContext _appRepository;

        public CollectionQuery(ApplicationDbContext appRepository)
        {
            _appRepository = appRepository;
        }

        public IQueryable<CollectionEntity> GetAll()
        {
            return _appRepository.Collections.AsQueryable();
        }

        public async Task<CollectionEntity> GetById(Guid id)
        {
            return await _appRepository.Set<CollectionEntity>().FindAsync(id);
        }
    }
}
