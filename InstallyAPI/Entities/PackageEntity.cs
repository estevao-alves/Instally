using System.ComponentModel.DataAnnotations.Schema;

namespace InstallyAPI.Models
{
    public class PackageEntity : BaseEntity
    {
        public string WingetId { get; set; }
        public string Name { get; set; }
        public string Publisher { get; set; }

        [NotMapped]
        public string[] Tags
        {
            get { return TagsString?.Split(','); }
            set { TagsString = string.Join(',', value); }
        }
        public string TagsString { get; set; }
        public string Description { get; set; }
        public string? Site { get; set; }
        public int VersionsLength { get; set; }
        public string LatestVersion { get; set; }
        public double Score { get; set; }

        public Guid? CollectionId { get; set; }

        [ForeignKey("CollectionId")]
        public CollectionEntity? Collection { get; set; }

        public PackageEntity() { }

        // Constructor that accepts a GUID
        public PackageEntity(Guid guid, string wingetId, string name, string publisher, string[] tags, string description, string site, int versionsLength, string latestVersion, double score)
            : this()
        {
            SetGuid(guid); // Use the provided GUID
            WingetId = wingetId;
            Name = name;
            Publisher = publisher;
            Tags = tags;
            Description = description;
            Site = site;
            VersionsLength = versionsLength;
            LatestVersion = latestVersion;
            Score = score;
        }

        public void AtualizarCollection(Guid collectionId)
        {
            CollectionId = collectionId;
        }
    }
}
