using System.Threading.Tasks;

namespace NureSEConsultations.Bot.Model
{
    public class DbContextFactory
    {
        private readonly string connectionString;

        private bool created;

        public DbContextFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<AppDbContext> CreateAsync()
        {
            var context = new AppDbContext(this.connectionString);
            if (!this.created)
            {
                await context.Database.EnsureCreatedAsync();
                this.created = true;
            }
            return context;
        }
    }
}
