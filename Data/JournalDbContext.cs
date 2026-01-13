using Microsoft.EntityFrameworkCore;
using JournalApp.Models;

namespace JournalApp.Data
{
    /// <summary>
    /// Database Context for the application (Entity Framework Core).
    /// [VIVA INFO]: Bridges the gap between C# objects and the SQLite database.
    /// It handles the 'M' (Model) and 'D' (Data) of the application.
    /// </summary>
    public class JournalDbContext : DbContext
    {
        // [DATA TABLES]: Each DbSet represents a table in the database.
        public DbSet<JournalEntry> JournalEntries { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<EntryTag> EntryTags { get; set; } = null!;
        public DbSet<AppSettings> AppSettings { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        public JournalDbContext(DbContextOptions<JournalDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// [DATABASE CONFIG]: Overriding OnConfiguring to define the SQLite file path.
        /// [VIVA INFO]: This uses 'LocalApplicationData' to ensure the database 
        /// is stored in a permanent, cross-platform location.
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "JournalApp",
                    "journal.db"
                );

                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        /// <summary>
        /// [SCHEMA MODELING]: Using Fluent API to define relationships and constraints.
        /// [VIVA INFO]: Fluent API is more powerful than using Data Annotations [...]
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // [RELATIONSHIP]: Many-to-Many configuration (Junction Table).
            // A Journal Entry can have many Tags, and a Tag can appear in many Entries.
            modelBuilder.Entity<EntryTag>()
                .HasKey(et => new { et.JournalEntryId, et.TagId });

            modelBuilder.Entity<EntryTag>()
                .HasOne(et => et.JournalEntry)
                .WithMany(e => e.EntryTags)
                .HasForeignKey(et => et.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EntryTag>()
                .HasOne(et => et.Tag)
                .WithMany(t => t.EntryTags)
                .HasForeignKey(et => et.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // [CONSTRAINT]: Ensures a user has only ONE entry per specific date. 
            // This is a business rule enforced at the database level.
            modelBuilder.Entity<JournalEntry>()
                .HasIndex(e => new { e.UserId, e.EntryDate })
                .IsUnique();

            // [RELATIONSHIP]: One-to-Many configuration.
            // One User can have many Journal Entries.
            modelBuilder.Entity<JournalEntry>()
                .HasOne(j => j.User)
                .WithMany(u => u.JournalEntries)
                .HasForeignKey(j => j.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // [IDENTITY]: Unique constraints for secure user accounts.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // [SEEDING]: Initializing the database with default tags, user, and settings.
            SeedPreBuiltTags(modelBuilder);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "Journaler",
                    Email = "journaler@example.com",
                    PasswordHash = "bypass",
                    Salt = "bypass",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                }
            );

            modelBuilder.Entity<AppSettings>().HasData(
                new AppSettings
                {
                    Id = 1,
                    Theme = "Dark",
                    EntriesPerPage = 12,
                    LastUpdated = DateTime.Now
                }
            );
        }

        /// <summary>
        /// Helper for pre-populating the tag table.
        /// </summary>
        private void SeedPreBuiltTags(ModelBuilder modelBuilder)
        {
            var preBuiltTags = TagDefinitions.GetAllPreBuiltTags();
            var tagEntities = new List<Tag>();

            for (int i = 0; i < preBuiltTags.Count; i++)
            {
                tagEntities.Add(new Tag
                {
                    Id = i + 1,
                    Name = preBuiltTags[i],
                    IsPreBuilt = true
                });
            }

            modelBuilder.Entity<Tag>().HasData(tagEntities);
        }
    }
}
