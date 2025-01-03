using System.ComponentModel.DataAnnotations.Schema;

namespace InstallyAPI.Models
{
    public class CollectionEntity : BaseEntity
    {
        public string Title { get; set; }

        [ForeignKey("UserId")]
        public Guid UserId { get; set; }

        public List<PackageEntity> Packages { get; set; } = new(); 

        public CollectionEntity() { }

        public CollectionEntity(string title, Guid userId, List<PackageEntity> packages)
            : this()
        {
            Title = title;
            UserId = userId;
            Packages = packages;
        }
    }
}
