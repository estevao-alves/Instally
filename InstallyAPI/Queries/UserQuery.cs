using InstallyAPI.Data;
using InstallyAPI.Models;
using InstallyAPI.Queries.Interfaces;

namespace InstallyAPI.Queries
{
    public class UserQuery : IUserQuery
    {
        private readonly ApplicationDbContext _appRepository;

        public UserQuery(ApplicationDbContext appRepository)
        {
            _appRepository = appRepository;
        }

        public IQueryable<UserEntity> GetAll()
        {
            return _appRepository.Users.AsQueryable();
        }

        public async Task<UserEntity> GetById(Guid id)
        {
            return await _appRepository.Set<UserEntity>().FindAsync(id);
        }
    }
}
