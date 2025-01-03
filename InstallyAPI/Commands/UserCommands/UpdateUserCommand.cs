using InstallyAPI.Models;
using MediatR;

namespace InstallyAPI.Commands.UserCommands
{
    public class UpdateUserCommand : IRequest<bool>
    {
        public string? Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserEntity User { get; set; }

        public UpdateUserCommand(string email, string password)
        {
            Password = password;
            Email = email;
        }

        public void AssociarDados(UserEntity user)
        {
            User = user;
        }
    }
}
