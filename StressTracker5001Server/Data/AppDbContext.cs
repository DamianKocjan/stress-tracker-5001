using Microsoft.EntityFrameworkCore;

namespace StressTracker5001Server.Data
{

    public class AppDbContext : DbContext
    {
        //public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=db.sqlite");
    }
}
