using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace NureSEConsultations.Bot.Model
{
    public class AppDbContext : DbContext
    {
        private readonly string connectionString;

        public AppDbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(this.connectionString, options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map table names
            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(e => e.ChatId);
                b.Property(e => e.StartTime).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
