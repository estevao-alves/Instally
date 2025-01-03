using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using InstallyAPI.Models;
using InstallyAPI.Repository.Interfaces;

namespace InstallyAPI.Data
{
    public class ApplicationDbContext : DbContext, IUnitOfWork
    {
        public ApplicationDbContext() { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<CollectionEntity> Collections { get; set; }
        public DbSet<PackageEntity> Packages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string dbPath;

                if (OperatingSystem.IsWindows())
                {
                    dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Instally", "./InstallyData.db");
                }
                else if (OperatingSystem.IsLinux())
                {
                    dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".Instally", "./InstallyData.db");
                }
                else
                {
                    dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Instally", "./InstallyData.db");
                }
                
                optionsBuilder.UseSqlite(dbPath);
                
                // Ensure the directory exists
                var directory = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Set the SQLite connection string
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
            
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /* Dummy Data, Delete for production */
            /*modelBuilder.Entity<PackageEntity>().HasData(
                new PackageEntity
                {
                    Guid = Guid.NewGuid(),
                    WingetId = "BlenderFoundation.Blender",
                    Name = "Blender",
                    Publisher = "Blender Foundation",
                    TagsString = "3d,animation,modelling,sculpting,vfx,render,rigging", 
                    Description = "Blender is the free and open source 3D creation suite. It supports the entirety of the 3D pipeline—modeling, rigging, animation, simulation, rendering, compositing and motion tracking, video editing and 2D animation pipeline.",
                    Site = "https://www.blender.org",
                    VersionsLength = 31,
                    LatestVersion = "3.4.1",
                    Score = 1000,
                    CollectionId = Guid.NewGuid()
                },
                new PackageEntity
                {
                    Guid = Guid.NewGuid(),
                    WingetId = "Figma.Figma",
                    Name = "Figma",
                    Publisher = "Figma, Inc.",
                    TagsString = "Vector,Illustration", 
                    Description = "The collaborative interface design tool Build better products as a team. Design, prototype, and gather feedback all in one place with Figma.",
                    Site = "https://www.figma.com",
                    VersionsLength = 1,
                    LatestVersion = "116.7.6",
                    Score = 998,
                    CollectionId = Guid.NewGuid()
                }
            );*/
            
            modelBuilder.Entity<UserEntity>().HasData(
                new UserEntity { 
                    Guid = Guid.NewGuid(), 
                    Email = "user1@example.com", 
                    Password = "password" 
                }
            );
            
            modelBuilder.Entity<UserEntity>(c =>
            {
                c.Property(x => x.Email).HasMaxLength(100);
                c.Property(x => x.Password).HasMaxLength(80);
            });

            base.OnModelCreating(modelBuilder);
        }

        public async Task<bool> Save()
        {
            try
            {
                await base.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                Exception innerException = ex.InnerException;
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
