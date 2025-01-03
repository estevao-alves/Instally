namespace InstallyAPI.Models
{
    public class UserEntity : BaseEntity
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public List<CollectionEntity> Collections { get; set; }

        public UserEntity(string email, string password)
        {
            Email = email;
            Password = password;
        }
        
        public UserEntity() { }

        public void Atualizar(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}
