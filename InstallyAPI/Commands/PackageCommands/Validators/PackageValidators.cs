using FluentValidation;

namespace InstallyAPI.Commands.PackageCommands.Validators
{
    public sealed class AddPackageValidator : AbstractValidator<AddPackagesCommand>
    {
        public AddPackageValidator()
        {
            RuleFor(c => c.Packages).NotEmpty().WithMessage("Informe o applicativo.");
        }
    }

    public sealed class AddToCollectionValidator : AbstractValidator<AddToCollectionCommand>
    {
        public AddToCollectionValidator()
        {
            RuleFor(c => c.CollectionId).NotEmpty().WithMessage("Informe a coleção.");
        }
    }
}
