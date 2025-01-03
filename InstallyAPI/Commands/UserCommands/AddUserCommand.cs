using MediatR;

namespace InstallyAPI.Commands.UserCommands
{
    public class AddUserCommand : IRequest<bool>
    {
        public string? Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public AddUserCommand(string email, string password)
        {
            Name = email.Split("@")[0];
            Password = password;
            Email = email;
        }
    }
}
