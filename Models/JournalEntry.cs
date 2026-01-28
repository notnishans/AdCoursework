using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JournalApp.Models
{
    /// <summary>
    /// Represents a single journal entry.
    /// [VIVA INFO]: Demonstrates 'Encapsulation' (properties) and 'Validation' (Data Annotations).
    /// </summary>
    public class JournalEntry
    {
        // [PRIMARY KEY]: Unique identifier automatically managed by SQLite.
        [Key]
        public int Id { get; set; }

        // [VALIDATION]: 'Required' ensures data cannot be null in the DB.
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        [StringLength(10000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 10000 characters")]
        public string Content { get; set; } = string.Empty;

        // [BUSINESS RULE]: The date this journal entry refers to.
        [Required(ErrorMessage = "Date is required")]
        public DateTime EntryDate { get; set; }

        [NotMapped]
        public DateTime? _entryDate
        {
            get => EntryDate;
            set => EntryDate = value ?? DateTime.Today;
        }

        // [METADATA]: System-generated timestamps for auditing.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string PrimaryMood { get; set; } = string.Empty;
        public MoodCategory PrimaryMoodCategory { get; set; }

        public string? SecondaryMood1 { get; set; }
        public MoodCategory? SecondaryMood1Category { get; set; }

        public string? SecondaryMood2 { get; set; }
        public MoodCategory? SecondaryMood2Category { get; set; }

        // [DATABASE DENORMALIZATION]: Storing tags as CSV for simple keyword search.
        public string? Tags { get; set; }

        // [CALCULATED DATA]: Determined during service logic, not by user input.
        public int WordCount { get; set; }

        // [SECURITY]: Optional PIN for per-journal vault protection.
        // If null or empty, the journal is not protected.
        public string? Pin { get; set; }

        // [FOREIGN KEY]: Links this entry to a specific User (One-to-Many).
        public int UserId { get; set; }

        // [NAVIGATION]: EF Core property to access related objects easily.
        public virtual User User { get; set; } = null!;
        public virtual ICollection<EntryTag> EntryTags { get; set; } = new List<EntryTag>();
    }

    /// <summary>
    /// Mood categories for classification
    /// </summary>
    public enum MoodCategory
    {
        Positive = 1,
        Neutral = 2,
        Negative = 3
    }
}
