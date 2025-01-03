using InstallyAPI.Models;
using MediatR;

namespace InstallyAPI.Commands.CollectionCommands
{
    public class AddCollectionCommand : IRequest<bool>
    {
        public string Title { get; set; }
        public Guid UserId { get; set; }
        public List<PackageEntity>? Packages { get; set; }

        public AddCollectionCommand(string title, Guid userId, List<PackageEntity>? packages)
        {
            UserId = userId;
            Title = title;
            Packages = packages;
        }
    }
}
