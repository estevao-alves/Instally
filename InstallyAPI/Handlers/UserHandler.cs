using Instally.App.Application;
using MediatR;
using System.Data;
using InstallyAPI.Commands.UserCommands;
using InstallyAPI.Models;
using InstallyAPI.Queries.Interfaces;
using InstallyAPI.Repository.Interfaces;

namespace InstallyAPI.Handlers;

public class UserHandler : IRequestHandler<AddUserCommand, bool>, IRequestHandler<UpdateUserCommand, bool>, IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IAppRepository<UserEntity> _userRepository;
    private readonly IUserQuery _userQuery;

    // Inject IUserQuery along with IAppRepository<UserEntity>
    public UserHandler(IAppRepository<UserEntity> userRepository, IUserQuery userQuery)
    {
        _userRepository = userRepository;
        _userQuery = userQuery;
    }

    public async Task<bool> Handle(AddUserCommand message, CancellationToken cancellationToken)
    {
        UserEntity user = new(message.Email, message.Password);

        _userRepository.Add(user);

        return await _userRepository.UnitOfWork.Save();
    }

    public async Task<bool> Handle(UpdateUserCommand message, CancellationToken cancellationToken)
    {
        // Use the injected IUserQuery directly
        UserEntity user = _userQuery.GetAll().FirstOrDefault();

        user.Atualizar(message.Email, message.Password);

        _userRepository.Update(user);

        message.AssociarDados(user);

        var resultado = await _userRepository.UnitOfWork.Save();

        return resultado;
    }

    public async Task<bool> Handle(DeleteUserCommand message, CancellationToken cancellationToken)
    {
        // Use the injected IUserQuery directly
        UserEntity user = _userQuery.GetAll().FirstOrDefault(u => u.Email == message.Email);

        _userRepository.Delete(user);

        var resultado = await _userRepository.UnitOfWork.Save();

        return resultado;
    }
}
