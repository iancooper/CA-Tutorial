using GreetingsCore.Model;
using Microsoft.EntityFrameworkCore;

namespace GreetingsCore.Adapters.Db
{
    public class GreetingContext : DbContext
    {
        public DbSet<Greeting> Greetings { get; set; }
        
        protected GreetingContext(){}

        public GreetingContext(DbContextOptions<GreetingContext> options) : base(options){}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory;Trusted_Connection=True;");
            }

            // Fixes issue with MySql connector reporting nested transactions not supported https://github.com/aspnet/EntityFrameworkCore/issues/7017
            //Database.AutoTransactionsEnabled = false;

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Greeting>()
                .Property(field => field.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Greeting>()
                .HasKey(field => field.Id);
        }
 
    }
}