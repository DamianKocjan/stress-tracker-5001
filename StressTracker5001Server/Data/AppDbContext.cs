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
        public DbSet<Tag> Tags { get; set; }
        public DbSet<CardTag> CardTags { get; set; }

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

            modelBuilder.Entity<Board>()
                .HasMany(b => b.Tags)
                .WithOne(t => t.Board)
                .HasForeignKey(t => t.BoardId);

            modelBuilder.Entity<Column>()
                .HasMany(c => c.Cards)
                .WithOne(c => c.Column)
                .HasForeignKey(c => c.ColumnId);

            modelBuilder.Entity<Card>()
                .HasOne(c => c.CreatedBy)
                .WithMany(u => u.CreatedCards)
                .HasForeignKey(c => c.CreatedById);

            // Many-to-Many relationship between Card and Tag via CardTag
            modelBuilder.Entity<CardTag>()
                .HasKey(ct => new { ct.CardId, ct.TagId });

            modelBuilder.Entity<CardTag>()
                .HasOne(ct => ct.Card)
                .WithMany(c => c.CardTags)
                .HasForeignKey(ct => ct.CardId);

            modelBuilder.Entity<CardTag>()
                .HasOne(ct => ct.Tag)
                .WithMany(t => t.CardTags)
                .HasForeignKey(ct => ct.TagId);
        }
    }
}
