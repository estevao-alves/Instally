using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InstallyAPI.Models
{
    public class BaseEntity
    {
        [Key]
        public Guid Guid { get; set; }

        [JsonIgnore]
        public DateTime CreatedAt { get; protected set; }
        [JsonIgnore]
        public DateTime UpdatedAt { get; protected set; }

        protected BaseEntity()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
        
        public void SetGuid(Guid guid)
        {
            if (Guid == Guid.Empty)
            {
                Guid = guid;
            }
        }
    }
}
