using System;
using System.Collections.Generic;
using System.Linq;
using JournalApp.Models;

namespace JournalApp.Services
{
    public class JournalService : IJournalService
    {
        private List<JournalEntry> _entries = new List<JournalEntry>();

        public JournalService()
        {
            // Seed some data for testing if needed, or keep empty
        }

        public List<JournalEntry> GetEntries()
        {
            return _entries.OrderByDescending(e => e.CreatedDate).ToList();
        }

        public JournalEntry? GetEntryByDate(DateTime date)
        {
            return _entries.FirstOrDefault(e => e.CreatedDate.Date == date.Date);
        }

        public JournalEntry? GetEntryById(Guid id)
        {
             return _entries.FirstOrDefault(e => e.Id == id);
        }

        public void SaveEntry(JournalEntry entry)
        {
            var existingEntry = _entries.FirstOrDefault(e => e.Id == entry.Id);

            if (existingEntry != null)
            {
                // Update
                existingEntry.Title = entry.Title;
                existingEntry.Content = entry.Content;
                existingEntry.PrimaryMood = entry.PrimaryMood;
                existingEntry.SecondaryMoods = entry.SecondaryMoods;
                existingEntry.Tags = entry.Tags;
                existingEntry.UpdatedDate = DateTime.Now;
            }
            else
            {
                // Create
                // Check if entry for this date already exists
                var entryForDate = GetEntryByDate(entry.CreatedDate);
                if (entryForDate != null)
                {
                    throw new InvalidOperationException($"An entry already exists for {entry.CreatedDate.ToShortDateString()}. Please edit the existing entry.");
                }

                _entries.Add(entry);
            }
        }

        public void DeleteEntry(Guid id)
        {
            var entry = _entries.FirstOrDefault(e => e.Id == id);
            if (entry != null)
            {
                _entries.Remove(entry);
            }
        }

        public List<Tag> GetUsedTags()
        {
            return _entries.SelectMany(e => e.Tags)
                           .GroupBy(t => t.Name)
                           .Select(g => g.First())
                           .ToList();
        }
    }
}