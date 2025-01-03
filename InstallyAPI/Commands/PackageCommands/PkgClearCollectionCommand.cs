using InstallyAPI.Models;
using MediatR;

namespace InstallyAPI.Commands.PackageCommands
{
    public class PkgClearCollectionCommand : IRequest<bool>
    {
        public PackageEntity Package { get; set; }

        public PkgClearCollectionCommand(PackageEntity package)
        {
            Package = package;
        }
    }
}
