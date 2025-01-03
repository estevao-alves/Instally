using InstallyAPI.Models;
using MediatR;

namespace InstallyAPI.Commands.PackageCommands
{
    public class AddPackagesCommand : IRequest<bool>
    {
        public List<PackageEntity> Packages { get; set; }
        public Guid? CollectionId { get; set; }

        public AddPackagesCommand(List<PackageEntity> packages)
        {
            Packages = packages;
        }
        
        public AddPackagesCommand(List<PackageEntity> packages, Guid? collectionId)
        {
            Packages = packages;
            CollectionId = collectionId;
        }
    }
}