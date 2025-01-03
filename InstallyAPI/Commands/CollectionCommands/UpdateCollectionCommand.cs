using InstallyAPI.Models;
using MediatR;

namespace InstallyAPI.Commands.CollectionCommands
{
    public class UpdateCollectionCommand : IRequest<bool>
    {
        public Guid CollectionId { get; set; }
        public string Title { get; set; }
        public List<PackageEntity>? Packages { get; set; }

        public UpdateCollectionCommand(Guid collectionId, string title)
        {
            CollectionId = collectionId;
            Title = title;
        }
        
        public UpdateCollectionCommand(Guid collectionId, string title, List<PackageEntity>? packages)
        {
            CollectionId = collectionId;
            Title = title;
            Packages = packages;
        }
    }
}