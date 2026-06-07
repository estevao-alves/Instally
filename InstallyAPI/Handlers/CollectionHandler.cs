using System.Diagnostics;
using MediatR;
using InstallyAPI.Commands.CollectionCommands;
using InstallyAPI.Data;
using InstallyAPI.Models;
using InstallyAPI.Queries.Interfaces;
using InstallyAPI.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InstallyAPI.Handlers;


public class CollectionHandler :
    IRequestHandler<AddCollectionCommand, bool>,
    IRequestHandler<DeleteCollectionCommand, bool>,
    IRequestHandler<UpdateCollectionCommand, bool>,
    IRequestHandler<AddManyToCollectionCommand, bool>

{
    private readonly ApplicationDbContext _context;
    private readonly IAppRepository<CollectionEntity> _collectionRepository;
    private readonly ICollectionQuery _collectionQuery;

    public CollectionHandler(
        ApplicationDbContext context,
        IAppRepository<CollectionEntity> collectionRepository,
        ICollectionQuery collectionQuery)
    {
        _context = context;
        _collectionRepository = collectionRepository;
        _collectionQuery = collectionQuery;
    }

    public async Task<bool> Handle(AddCollectionCommand message, CancellationToken cancellationToken)
    {
        // Create empty collection first
        var collection = new CollectionEntity(message.Title, message.UserId, new List<PackageEntity>());

        if (message.Packages != null)
        {
            foreach (var pkg in message.Packages)
            {
                var existingPkg = await _context.Packages
                    .FirstOrDefaultAsync(p => p.Guid == pkg.Guid);

                if (existingPkg != null)
                {
                    collection.Packages.Add(existingPkg);
                }
            }
        }

        _collectionRepository.Add(collection);

        return await _collectionRepository.UnitOfWork.Save();
    }
    
    public async Task<bool> Handle(
        AddManyToCollectionCommand message,
        CancellationToken cancellationToken)
    {
        var collection = await _context.Collections
            .Include(c => c.Packages)
            .FirstOrDefaultAsync(
                c => c.Guid == message.CollectionGuid,
                cancellationToken);

        if (collection == null)
            return false;

        var packages = await _context.Packages
            .Where(p => message.PackageGuids.Contains(p.Guid))
            .ToListAsync(cancellationToken);

        var existingGuids = collection.Packages
            .Select(p => p.Guid)
            .ToHashSet();

        foreach (var package in packages)
        {
            if (!existingGuids.Contains(package.Guid))
            {
                collection.Packages.Add(package);
            }
        }

        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> Handle(
        DeleteCollectionCommand message,
        CancellationToken cancellationToken)
    {
        var collection = await _context.Collections
            .Include(c => c.Packages)
            .FirstOrDefaultAsync(
                c => c.Guid == message.CollectionId,
                cancellationToken);

        if (collection == null)
            return false;

        // Clear many-to-many relationships first
        collection.Packages.Clear();

        // Delete collection
        _context.Collections.Remove(collection);

        return await _context.SaveChangesAsync(cancellationToken) > 0;
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
