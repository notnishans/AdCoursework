using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JournalApp.Models
{
    public class JournalEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty; // Markdown or Rich Text

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        [Required]
        public MoodType PrimaryMood { get; set; } = MoodType.Neutral;

        public List<MoodType> SecondaryMoods { get; set; } = new List<MoodType>();

        public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}