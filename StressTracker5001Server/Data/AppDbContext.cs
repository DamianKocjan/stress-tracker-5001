using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Models;

namespace StressTracker5001Server.Data
{

    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Column> Columns { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<CardAssignment> CardAssignments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<CardTag> CardTags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<BoardMember> BoardMembers { get; set; }
        public DbSet<BoardInvite> BoardInvites { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

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
                .HasMany(u => u.PasswordResetTokens)
                .WithOne(prt => prt.User)
                .HasForeignKey(prt => prt.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.EmailVerificationTokens)
                .WithOne(evt => evt.User)
                .HasForeignKey(evt => evt.UserId);

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

            modelBuilder.Entity<Card>()
                .HasMany(c => c.CardAssignments)
                .WithOne(ca => ca.Card)
                .HasForeignKey(ca => ca.CardId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.CardAssignments)
                .WithOne(ca => ca.User)
                .HasForeignKey(ca => ca.UserId);

            modelBuilder.Entity<CardAssignment>()
                .HasIndex(ca => new { ca.CardId, ca.UserId })
                .IsUnique();

            // Attachments relationships
            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Card)
                .WithMany(c => c.Attachments)
                .HasForeignKey(a => a.CardId);

            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.UploadedBy)
                .WithMany(u => u.UserAttachments)
                .HasForeignKey(a => a.UploadedById);

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

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Card)
                .WithMany(c => c.Comments)
                .HasForeignKey(c => c.CardId);

            modelBuilder.Entity<BoardMember>()
                .HasOne(bm => bm.Board)
                .WithMany(b => b.Members)
                .HasForeignKey(bm => bm.BoardId);

            modelBuilder.Entity<BoardMember>()
                .HasIndex(bm => new { bm.BoardId, bm.UserId })
                .IsUnique();

            modelBuilder.Entity<BoardMember>()
                .HasOne(bm => bm.User)
                .WithMany(u => u.BoardMemberships)
                .HasForeignKey(bm => bm.UserId);

            modelBuilder.Entity<BoardInvite>()
                .HasIndex(bi => bi.Token)
                .IsUnique();

            modelBuilder.Entity<BoardInvite>()
                .HasOne(bi => bi.Board)
                .WithMany(b => b.Invites)
                .HasForeignKey(bi => bi.BoardId);

            modelBuilder.Entity<BoardInvite>()
                .HasOne(bi => bi.GeneratedByUser)
                .WithMany(u => u.BoardInvites)
                .HasForeignKey(bi => bi.GeneratedByUserId);

            modelBuilder.Entity<ActivityLog>()
                .HasOne(al => al.Board)
                .WithMany(b => b.ActivityLogs)
                .HasForeignKey(al => al.BoardId);

            modelBuilder.Entity<ActivityLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.ActivityLogs)
                .HasForeignKey(al => al.UserId);

            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(prt => prt.TokenHash);

            modelBuilder.Entity<EmailVerificationToken>()
                .HasIndex(evt => evt.TokenHash);
        }
    }
}
