using MediatR;

namespace InstallyAPI.Commands.UserCommands
{
    public class DeleteUserCommand : IRequest<bool>
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public DeleteUserCommand(string email, string password)
        {
            Password = password;
            Email = email;
        }
    }
}
