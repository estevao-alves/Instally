using MediatR;
using Instally.App.Application;
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
            PackageEntity package = new(pkg.Guid, pkg.WingetId, pkg.Name, pkg.Publisher, pkg.Tags, pkg.Description, pkg.Site, pkg.VersionsLength, pkg.LatestVersion, pkg.Score);

            _packageRepository.Add(package);
        }

        return await _packageRepository.UnitOfWork.Save();
    }

    public async Task<bool> Handle(AddToCollectionCommand message, CancellationToken cancellationToken)
    {
        if (message.CollectionId != null)
        {
            PackageEntity package = await _packageQuery.GetById(message.PackageId);
            
            package.CollectionId = message.CollectionId;

            _packageRepository.Update(package);

            return await _packageRepository.UnitOfWork.Save();
        }

        return false;
    }
    
    public async Task<bool> Handle(PkgClearCollectionCommand message, CancellationToken cancellationToken)
    {
        message.Package.CollectionId = null;
        _packageRepository.Update(message.Package);

        return await _packageRepository.UnitOfWork.Save();
    }
}
