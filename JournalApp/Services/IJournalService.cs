using System;
using System.Collections.Generic;
using JournalApp.Models;

namespace JournalApp.Services
{
    public interface IJournalService
    {
        List<JournalEntry> GetEntries();
        JournalEntry? GetEntryByDate(DateTime date);
        JournalEntry? GetEntryById(Guid id);
        void SaveEntry(JournalEntry entry);
        void DeleteEntry(Guid id);
        
        List<Tag> GetUsedTags();
    }
}