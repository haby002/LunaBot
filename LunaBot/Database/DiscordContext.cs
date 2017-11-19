using Microsoft.EntityFrameworkCore;

namespace LunaBot.Database
{
    public class DiscordContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseMySql(string.Format(@"Server=localhost;database={0};uid={1};pwd={2};", SecretStrings.database, SecretStrings.user, SecretStrings.password));
    }
}
