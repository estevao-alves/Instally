using System.IO;
using System.Reflection;
using FluentValidation;
using InstallyAPI.Commands.CollectionCommands;
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
using InstallyApp.Services;
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
    public static UserEntity UserAuthenticated { get; set; }
    public static List<PackageEntity> Packages { get; set; } = new();
    public static List<CollectionEntity> Collections { get; set; }

    public static DebugStatus Debug;
    public static MainWindow Main;

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            PlatformService.Initialize();
            
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
            
            // Ensure DB is innitialized
            var serviceProvider = serviceCollection.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                // Clear EF migration lock if it already exists
                // That avoids keeping the migration waiting forever in case the old migration didn't finish correctly
                try
                {
                    db.Database.ExecuteSqlRaw("DELETE FROM __EFMigrationsLock");
                }
                catch
                {
                    // Lock do not exist yet, it's clean
                }

                db.Database.Migrate();
            }
            catch (Exception migrationEx)
            {
                File.WriteAllText(Path.Combine(appDataPath, "migration_error.txt"), $"=== MIGRATION FAILED ===\n{migrationEx}\n");

                try
                {
                    if (File.Exists(dbPath))
                    {
                        var backupPath = Path.Combine(appDataPath, $"InstallyData_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");

                        File.Copy(dbPath,backupPath);
                    }
                }
                catch (Exception backupEx)
                {
                    File.AppendAllText(Path.Combine(appDataPath, "migration_error.txt"), $"\n=== BACKUP FAILED ===\n{backupEx}\n");
                }

                db.Database.EnsureDeleted();
                db.Database.Migrate();
            }
            
            // MediatR and Handlers
            serviceCollection.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(CollectionHandler))); // Register CollectionHandler
                cfg.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(PackageHandler)));  // Register PackageHandler
                cfg.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(UserHandler)));    // Register UserHandler
            });
            
            // Queries
            serviceCollection.AddScoped<IUserQuery, UserQuery>();
            serviceCollection.AddScoped<IPackageQuery, PackageQuery>();
            serviceCollection.AddScoped<ICollectionQuery, CollectionQuery>();
            
            // Services
            serviceCollection.AddSingleton<ApiHostService>();
            serviceCollection.AddSingleton<ApiService>();
            serviceCollection.AddScoped<SyncService>();
            serviceCollection.AddScoped(typeof(IAppRepository<>), typeof(AppRepository<>));
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
            // var manageUserViewModel = Services.GetRequiredService<ManageUserViewModel>();
            
            var mainWindow = Services.GetRequiredService<MainWindow>();
            NavigationService = new NavigationService(mainWindow);

            mainWindow.DataContext = mainWindow;
            desktop.MainWindow = mainWindow;

            DisableAvaloniaDataAnnotationValidation();
            
            desktop.Exit += (_, _) =>
            {
                var apiHost = Services.GetRequiredService<ApiHostService>();
                apiHost.Stop();
            };
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
