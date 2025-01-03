using InstallyAPI.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace InstallyAPI.Repository.Interfaces
{
    public interface IRepository<T> : IDisposable where T : BaseEntity
    {
        IUnitOfWork UnitOfWork { get; }
        DatabaseFacade Database { get; }
    }
}
