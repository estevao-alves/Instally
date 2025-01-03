using MediatR;

namespace InstallyAPI.Commands.PackageCommands
{
    public class AddToCollectionCommand : IRequest<bool>
    {
        public Guid PackageId { get; set; }
        public Guid CollectionId { get; set; }

        public AddToCollectionCommand(Guid packageId, Guid collectionId)
        {
            PackageId = packageId;
            CollectionId = collectionId;
        }
    }
}
