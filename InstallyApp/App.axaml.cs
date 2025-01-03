using System.IO;
using System.Reflection;
using FluentValidation;
using InstallyAPI.Commands.PackageCommands;
using InstallyAPI.Commands.PackageCommands.Validators;
using InstallyAPI.Commands.UserCommands;
using InstallyAPI.Commands.UserCommands.Validators;
using InstallyAPI.Data;
using InstallyAPI.Handlers;
using InstallyAPI.Models;
using InstallyAPI.Queries;
using InstallyAPI.Queries.Interfaces;
using InstallyAPI.Repository;
using InstallyAPI.Repository.Interfaces;
using InstallyApp.DataServices;
using InstallyApp.Models.ViewModels;
using InstallyApp.Pages;
using InstallyApp.ViewModels;
using InstallyApp.Views.Components;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InstallyApp;

public class App : Application
{
    public static INavigationService NavigationService { get; private set; }
    public static IRestDataService DataService { get; private set; }
    public static IServiceProvider Services { get; private set; }
    public static IMediator Mediator { get; set; }
    
    public static IServiceProvider ServiceProvider { get; set; }
    
    public static UserEntity UserAuthenticated { get; set; }
    public static List<PackageEntity> Packages { get; set; }
    public static List<CollectionEntity> Collections { get; set; }

    public static DebugStatus Debug;
    public static MainWindow Main;

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Attach global exception handlers
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                File.AppendAllText("error.log", $"Unhandled Exception: {e.ExceptionObject}\n");
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                File.AppendAllText("error.log", $"Unobserved Task Exception: {e.Exception}\n");
                e.SetObserved();
            };
            
            // Add DataBase
            var serviceCollection = new ServiceCollection();
            
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "Instally");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            var dbPath = Path.Combine(appDataPath, "InstallyData.db");
            
            serviceCollection.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = $"Data Source={dbPath}";
                options.UseSqlite(connectionString);
            });
            
            // Apply database migrations at runtime
            using (var scope = serviceCollection.BuildServiceProvider().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
            }

            // MediatR
            serviceCollection.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });
            
            // Handlers
            serviceCollection.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(CollectionHandler))); // Register CollectionHandler
                cfg.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(PackageHandler)));  // Register PackageHandler
                cfg.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(UserHandler)));    // Register UserHandler
            });
            
            // IQuery
            serviceCollection.AddScoped(typeof(IAppRepository<>), typeof(AppRepository<>));
            serviceCollection.AddScoped<ICollectionQuery, CollectionQuery>();
            serviceCollection.AddScoped<IUserQuery, UserQuery>();
            serviceCollection.AddScoped<IPackageQuery, PackageQuery>();
            serviceCollection.AddHttpClient<IRestDataService, RestDataService>();
            
            // Validators
            serviceCollection.AddScoped<IValidator<AddUserCommand>, AddUserValidator>();
            serviceCollection.AddScoped<IValidator<UpdateUserCommand>, UpdateUserValidator>();
            serviceCollection.AddScoped<IValidator<DeleteUserCommand>, DeleteUserValidator>();
            serviceCollection.AddScoped<IValidator<AddPackagesCommand>, AddPackageValidator>();
            serviceCollection.AddScoped<IValidator<AddToCollectionCommand>, AddToCollectionValidator>();
            
            // Pages/Windows
            serviceCollection.AddSingleton<MainPage>();
            serviceCollection.AddSingleton<MainWindow>();
            serviceCollection.AddSingleton<AppSearch>();
            serviceCollection.AddSingleton<AppCollection>();
            serviceCollection.AddTransient<ManageUsersPage>();
            serviceCollection.AddSingleton<MainPageViewModel>();
            serviceCollection.AddSingleton<ManageUserViewModel>();
            serviceCollection.AddSingleton<INavigationService, NavigationService>();

            // Initialize
            Services = serviceCollection.BuildServiceProvider();
            DataService = Services.GetRequiredService<IRestDataService>();
            Mediator = Services.GetRequiredService<IMediator>();
            var mainWindow = Services.GetRequiredService<MainWindow>();
            // var manageUserViewModel = Services.GetRequiredService<ManageUserViewModel>();
            
            NavigationService = new NavigationService(mainWindow);

            mainWindow.DataContext = mainWindow;

            DisableAvaloniaDataAnnotationValidation();
            
            /*
            Avalonia.Diagnostics.DevToolsExtensions.AttachDevTools(this);
            */

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove) 
            BindingPlugins.DataValidators.Remove(plugin);
    }
}
