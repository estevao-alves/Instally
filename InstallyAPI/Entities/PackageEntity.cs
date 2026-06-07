using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace InstallyAPI.Models
{
    public class PackageEntity : BaseEntity
    {
        // use dictionary for IDs

        public string PackageIdsJson { get; set; } = "{}";

        [NotMapped]
        public Dictionary<string, string> PackageIds
        {
            get => string.IsNullOrEmpty(PackageIdsJson)
                ? new Dictionary<string, string>()
                : JsonSerializer.Deserialize<Dictionary<string, string>>(PackageIdsJson)!;

            set => PackageIdsJson = JsonSerializer.Serialize(value);
        }
        
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
        public string? Icon { get; set; }
        public List<string>? Screenshots { get; set; }
        public int VersionsLength { get; set; }
        public string LatestVersion { get; set; }
        public double Score { get; set; }

        public Guid? CollectionId { get; set; }

        [ForeignKey("CollectionId")]
        public CollectionEntity? Collection { get; set; }

        public PackageEntity() { }

        public PackageEntity(Guid guid, Dictionary<string, string> packageIds, string name, string publisher, string[] tags, string description, string site, string icon, List<string> screenshots, int versionsLength, string latestVersion, double score)
            : this()
        {
            SetGuid(guid);
            PackageIds = packageIds;
            Name = name;
            Publisher = publisher;
            Tags = tags;
            Description = description;
            Site = site;
            Icon = icon;
            Screenshots = screenshots;
            VersionsLength = versionsLength;
            LatestVersion = latestVersion;
            Score = score;
        }
    }
}
