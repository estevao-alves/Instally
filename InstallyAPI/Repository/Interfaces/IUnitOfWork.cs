namespace InstallyAPI.Repository.Interfaces
{
    public interface IUnitOfWork
    {
        Task<bool> Save();
    }
}