using InstallyAPI.Models;

namespace InstallyAPI.Queries.Interfaces
{
    public interface IUserQuery
    {
        IQueryable<UserEntity> GetAll();
        Task<UserEntity> GetById(Guid id);
    }
}
