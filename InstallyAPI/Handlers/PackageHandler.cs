using MediatR;
using InstallyAPI.Commands.PackageCommands;
using InstallyAPI.Models;
using InstallyAPI.Queries.Interfaces;
using InstallyAPI.Repository.Interfaces;

namespace InstallyAPI.Handlers;

public class PackageHandler : IRequestHandler<AddPackagesCommand, bool>, IRequestHandler<AddToCollectionCommand, bool>, IRequestHandler<PkgClearCollectionCommand, bool> 
{
    private readonly IAppRepository<PackageEntity> _packageRepository;
    private readonly IPackageQuery _packageQuery;
    private readonly ICollectionQuery _collectionQuery;

    // Inject IPackageQuery and ICollectionQuery along with IAppRepository<PackageEntity>
    public PackageHandler(IAppRepository<PackageEntity> packageRepository, IPackageQuery packageQuery, ICollectionQuery collectionQuery)
    {
        _packageRepository = packageRepository;
        _packageQuery = packageQuery;
        _collectionQuery = collectionQuery;
    }

    public async Task<bool> Handle(AddPackagesCommand message, CancellationToken cancellationToken)
    {
        foreach (PackageEntity pkg in message.Packages)
        {
            var existing = await _packageQuery.GetById(pkg.Guid);

            
            if (existing != null)
            {
                existing.Name = pkg.Name;
                existing.Publisher = pkg.Publisher;
                existing.TagsString = pkg.TagsString;
                existing.Description = pkg.Description;
                existing.Site = pkg.Site;
                existing.Icon = pkg.Icon;
                existing.Screenshots = pkg.Screenshots;
                existing.VersionsLength = pkg.VersionsLength;
                existing.LatestVersion = pkg.LatestVersion;
                existing.Score = pkg.Score;

                // Safe dictionary handling
                if (existing.PackageIds == null)
                    existing.PackageIds = new Dictionary<string, string>();

                if (pkg.PackageIds != null)
                {
                    foreach (var kvp in pkg.PackageIds)
                    {
                        existing.PackageIds[kvp.Key] = kvp.Value;
                    }
                }
            }
            else
            {
                _packageRepository.Add(pkg);
            }
        }

        return await _packageRepository.UnitOfWork.Save();
    }

    public async Task<bool> Handle(AddToCollectionCommand message, CancellationToken cancellationToken)
    {
        if (message.CollectionId != null)
        {
            PackageEntity package = await _packageQuery.GetById(message.PackageId);
            
            package.CollectionId = message.CollectionId;

            return await _packageRepository.UnitOfWork.Save();
        }

        return false;
    }
    
    public async Task<bool> Handle(
        PkgClearCollectionCommand message,
        CancellationToken cancellationToken)
    {
        var package = await _packageQuery
            .GetById(message.Package.Guid);

        if (package == null)
            return false;

        package.CollectionId = null;

        return await _packageRepository
            .UnitOfWork
            .Save();
    }
}
