using FluentValidation;
using InstallyAPI.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Instally.App.Application;
using Microsoft.AspNetCore.Mvc;
using InstallyAPI.Repository.Interfaces;
using InstallyAPI.Repository;
using InstallyAPI.Commands.UserCommands.Behaviors;
using InstallyAPI.Queries;
using InstallyAPI.Queries.Interfaces;
using InstallyAPI.Commands.UserCommands;
using InstallyAPI.Commands.UserCommands.Validators;
using InstallyAPI.Commands.PackageCommands;
using InstallyAPI.Commands.PackageCommands.Validators;
using InstallyAPI.Models;

var builder = WebApplication.CreateBuilder(args);

IServiceCollection ConfigureServices()
{
    builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddScoped(typeof(IAppRepository<>), typeof(AppRepository<>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    builder.Services.AddScoped<ICollectionQuery, CollectionQuery>();
    builder.Services.AddScoped<IPackageQuery, PackageQuery>();
    builder.Services.AddScoped<IUserQuery, UserQuery>();

    // UserCommands Validators
    builder.Services.AddScoped<IValidator<AddUserCommand>, AddUserValidator>();
    builder.Services.AddScoped<IValidator<UpdateUserCommand>, UpdateUserValidator>();
    builder.Services.AddScoped<IValidator<DeleteUserCommand>, DeleteUserValidator>();

    // PackageCommands Validators
    builder.Services.AddScoped<IValidator<AddPackagesCommand>, AddPackageValidator>();
    builder.Services.AddScoped<IValidator<AddToCollectionCommand>, AddToCollectionValidator>();

    builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<Program>());

    return builder.Services;
}

Master.ServiceProvider = ConfigureServices().BuildServiceProvider();
Master.Mediator = Master.ServiceProvider.GetRequiredService<IMediator>();

var app = builder.Build();

app.MapGet("api/user", async (IMediator mediator, ApplicationDbContext dbContext) =>
{
    var userQuery = Master.ServiceProvider.GetService<IUserQuery>();
    List<UserEntity> users = userQuery.GetAll().ToList();

    return Results.Ok(users);
});

app.MapPost("api/user", async (IMediator mediator, ApplicationDbContext dbContext, [FromBody] UserEntity user) =>
{
    try
    {
        AddUserCommand addCommand = new(user.Email, user.Password);
        var result = await Master.Mediator.Send(addCommand);

        return Results.Created($"api/user/createdUserId", result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPut("api/user/{id}", async (IMediator mediator, ApplicationDbContext dbContext, Guid id, [FromBody] UserEntity user) =>
{
    await Master.Mediator.Send(new UpdateUserCommand(user.Email, user.Password));
    return Results.NoContent();
});

app.MapDelete("api/user/{id}", async (IMediator mediator, ApplicationDbContext dbContext, Guid id) =>
{
    var userQuery = Master.ServiceProvider.GetService<IUserQuery>();
    UserEntity user = await userQuery.GetAll().FirstOrDefaultAsync(x => x.Guid == id);

    await Master.Mediator.Send(new DeleteUserCommand(user.Email, user.Password));
    return Results.NoContent();
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
    options.DocumentTitle = "My Swagger";
});

app.Run();