using MediatR;
using Instally.App.Application;
using InstallyAPI.Commands.CollectionCommands;
using InstallyAPI.Models;
using InstallyAPI.Queries;
using InstallyAPI.Queries.Interfaces;
using InstallyAPI.Repository.Interfaces;

namespace InstallyAPI.Handlers;

public class CollectionHandler :
    IRequestHandler<AddCollectionCommand, bool>,
    IRequestHandler<DeleteCollectionCommand, bool>,
    IRequestHandler<UpdateCollectionCommand, bool>

{
    private readonly IAppRepository<CollectionEntity> _collectionRepository;
    private readonly ICollectionQuery _collectionQuery;

    // Inject ICollectionQuery along with IAppRepository<CollectionEntity>
    public CollectionHandler(IAppRepository<CollectionEntity> collectionRepository, ICollectionQuery collectionQuery)
    {
        _collectionRepository = collectionRepository;
        _collectionQuery = collectionQuery;
    }

    public async Task<bool> Handle(AddCollectionCommand message, CancellationToken cancellationToken)
    {
        var collection = new CollectionEntity(message.Title, message.UserId, message.Packages);

        _collectionRepository.Add(collection);

        return await _collectionRepository.UnitOfWork.Save();
    }

    public async Task<bool> Handle(DeleteCollectionCommand message, CancellationToken cancellationToken)
    {
        // Use the injected ICollectionQuery directly
        var collection = await _collectionQuery.GetById(message.CollectionId);

        if (collection != null)
        {
            _collectionRepository.Delete(collection);
            return await _collectionRepository.UnitOfWork.Save();
        }

        return false;
    }
    
    public async Task<bool> Handle(UpdateCollectionCommand message, CancellationToken cancellationToken)
    {
        var collection = await _collectionQuery.GetById(message.CollectionId);

        if (collection != null)
        {
            collection.Title = message.Title;
            if (message.Packages != null) collection.Packages = message.Packages;

            _collectionRepository.Update(collection);
            return await _collectionRepository.UnitOfWork.Save();
        }

        return false;
    }
}
