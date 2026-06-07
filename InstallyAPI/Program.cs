using System.Diagnostics;
using FluentValidation;
using InstallyAPI.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

var port = Environment.GetEnvironmentVariable("INSTALLY_PORT") ?? "23842";

builder.WebHost.UseUrls($"http://localhost:{port}");

// REGISTER SERVICES
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    var appDataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Instally"
    );

    if (!Directory.Exists(appDataPath))
        Directory.CreateDirectory(appDataPath);

    var dbPath = Path.Combine(appDataPath, "InstallyData.db");

    opt.UseSqlite($"Data Source={dbPath}");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped(typeof(IAppRepository<>), typeof(AppRepository<>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddScoped<ICollectionQuery, CollectionQuery>();
builder.Services.AddScoped<IPackageQuery, PackageQuery>();
builder.Services.AddScoped<IUserQuery, UserQuery>();

// Validators
builder.Services.AddScoped<IValidator<AddUserCommand>, AddUserValidator>();
builder.Services.AddScoped<IValidator<UpdateUserCommand>, UpdateUserValidator>();
builder.Services.AddScoped<IValidator<DeleteUserCommand>, DeleteUserValidator>();

builder.Services.AddScoped<IValidator<AddPackagesCommand>, AddPackageValidator>();
builder.Services.AddScoped<IValidator<AddToCollectionCommand>, AddToCollectionValidator>();

builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssemblyContaining<Program>());

var app = builder.Build();


// ✅ APPLY MIGRATIONS + SEED
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    context.Database.Migrate();
    DbSeeder.Seed(context);
}


// -------------------- ROUTES --------------------

// GET packages
app.MapGet("api/package", (IPackageQuery query) =>
{
    return Results.Ok(query.GetAll().ToList());
});

// GET collection
app.MapGet("api/collection", (ICollectionQuery query) =>
{
    return Results.Ok(query.GetAll().ToList());
});

// GET users
app.MapGet("api/user", (IUserQuery userQuery) =>
{
    var users = userQuery.GetAll().ToList();
    return Results.Ok(users);
});

// POST user
app.MapPost("api/user", async (IMediator mediator, [FromBody] UserEntity user) =>
{
    try
    {
        var result = await mediator.Send(new AddUserCommand(user.Email, user.Password));
        return Results.Created($"api/user/{result}", result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// PUT user
app.MapPut("api/user/{id}", async (IMediator mediator, Guid id, [FromBody] UserEntity user) =>
{
    await mediator.Send(new UpdateUserCommand(user.Email, user.Password));
    return Results.NoContent();
});

// DELETE user
app.MapDelete("api/user/{id}", async (IUserQuery userQuery, IMediator mediator, Guid id) =>
{
    var user = await userQuery.GetAll().FirstOrDefaultAsync(x => x.Guid == id);

    if (user is null)
        return Results.NotFound();

    await mediator.Send(new DeleteUserCommand(user.Email, user.Password));
    return Results.NoContent();
});


// ----------- Guarantee that the API closes when main app is killed ------------
if (args.Length > 0 && int.TryParse(args[0], out int parentPid))
{
    _ = Task.Run(async () =>
    {
        while (true)
        {
            try
            {
                Process.GetProcessById(parentPid);
            }
            catch
            {
                Environment.Exit(0);
            }

            await Task.Delay(2000);
        }
    });
}

// -------------------- SWAGGER --------------------

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
    options.DocumentTitle = "My Swagger";
});

app.Run();