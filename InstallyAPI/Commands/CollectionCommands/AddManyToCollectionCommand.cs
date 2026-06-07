using MediatR;

namespace InstallyAPI.Commands.CollectionCommands;

public class AddManyToCollectionCommand : IRequest<bool>
{
    public List<Guid> PackageGuids { get; set; }

    public Guid CollectionGuid { get; set; }

    public AddManyToCollectionCommand(
        List<Guid> packageGuids,
        Guid collectionGuid)
    {
        PackageGuids = packageGuids;
        CollectionGuid = collectionGuid;
    }
}