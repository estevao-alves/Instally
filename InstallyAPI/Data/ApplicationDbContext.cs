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
            // Suppress the PendingModelChangesWarning until find out what's causing the mismatch, making ef thinks the migration changed
            // But do not forget to make a new migration everytime you changed an entity
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
