using InstallyAPI.Models;

namespace InstallyAPI.Data
{
    // Seeder is to safely insert default data avoid migration conflicts on and not interfere with migrations
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Users.Any())
            {
                context.Users.Add(new UserEntity("user1@example.com", "password"));
                context.SaveChanges();
            }
        }
    }
}