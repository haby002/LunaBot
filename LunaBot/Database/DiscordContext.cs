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
                .UseMySql(@"Server=localhost;database=DiscordContext;uid=root;pwd=;");
    }
}
