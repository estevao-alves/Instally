using InstallyAPI.Models;

namespace InstallyAPI.Queries.Interfaces
{
    public interface ICollectionQuery
    {
        IQueryable<CollectionEntity> GetAll();
        Task<CollectionEntity> GetById(Guid id);
    }
}
