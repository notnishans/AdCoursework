using System.ComponentModel.DataAnnotations;

namespace JournalApp.Models
{
    /// <summary>
    /// User account model.
    /// [VIVA INFO]: Central to identity management. This model stores securely 
    /// hashed credentials, never plain text.
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        // [SECURITY]: We store the HASH of the password, not the password itself.
        [Required(ErrorMessage = "Password is required")]
        public string PasswordHash { get; set; } = string.Empty;

        // [SECURITY]: Use a Salt to ensure even users with same password have different hashes.
        public string Salt { get; set; } = string.Empty;

        // [MAUI FEATURE]: Stores state of the app-level PIN protection.
        public bool HasJournalPin { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // [NAVIGATION]: One-to-Many Relationship. 
        // A user 'owns' many journal entries.
        public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
    }
}