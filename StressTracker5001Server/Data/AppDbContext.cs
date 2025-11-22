using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Models;

namespace StressTracker5001Server.Data
{

    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Column> Columns { get; set; }
        public DbSet<Card> Cards { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=db.sqlite");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Boards)
                .WithOne(b => b.Owner)
                .HasForeignKey(b => b.OwnerId);

            modelBuilder.Entity<Board>()
                .HasIndex(b => b.OwnerId);

            modelBuilder.Entity<Board>()
                .HasMany(b => b.Columns)
                .WithOne(c => c.Board)
                .HasForeignKey(c => c.BoardId);

            modelBuilder.Entity<Column>()
                .HasMany(c => c.Cards)
                .WithOne(c => c.Column)
                .HasForeignKey(c => c.ColumnId);
        }
    }
}
