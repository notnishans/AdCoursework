using JournalApp.Models;
using JournalApp.Data;
using Microsoft.EntityFrameworkCore;

namespace JournalApp.Services
{
    /// <summary>
    /// Service for managing journal entries
    /// Demonstrates business logic layer abstraction and encapsulation
    /// </summary>
    /// <summary>
    /// Service for managing journal entries.
    /// This class demonstrates the Single Responsibility Principle (SRP) by 
    /// handling ONLY journal-related business logic. It provides an Abstraction layer,
    /// meaning the UI components don't need to know HOW the data is saved or retrieved.
    /// </summary>
    public class JournalService
    {
        //Use of Private Read-Only field for the database context.
        // This ensures the database connection cannot be changed once the service is started.
        private readonly JournalDbContext _context;

        public JournalService(JournalDbContext context)
        {
            // [VIVA INFO]: DEPENDENCY INJECTION (DI). The system provides the 'JournalDbContext'
            // through the constructor. This makes the code modular, loosely coupled, and 
            // much easier to unit test.
            _context = context;
            
            // Ensure the default user exists for this simplified version
            _ = EnsureDefaultUserExistsAsync();
        }

        private async Task EnsureDefaultUserExistsAsync()
        {
            try
            {
                var user = await _context.Users.FindAsync(1);
                if (user == null)
                {
                    user = new User
                    {
                        Id = 1,
                        Username = "Journaler",
                        Email = "journaler@example.com",
                        PasswordHash = "bypass",
                        Salt = "bypass",
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to ensure default user: {ex.Message}");
            }
        }

        /// <summary>
        /// CREATE: Adds a new journal entry to the database.
        /// [VIVA INFO]: Demonstrates validation logic and business rules before data persistence.
        /// </summary>
        public async Task<bool> CreateEntryAsync(JournalEntry entry, int userId)
        {
            try
            {
                // [BUSINESS LOGIC]: Prevent duplicate entries for the same date.
                // We check if an entry already exists for this date and user.
                var start = entry.EntryDate.Date;
                var end = start.AddDays(1);
                var existingEntry = await _context.JournalEntries
                    .FirstOrDefaultAsync(e => e.EntryDate >= start && e.EntryDate < end && e.UserId == userId);

                if (existingEntry != null)
                {
                    Console.WriteLine($"[INFO] CreateEntryAsync: Entry already exists for date {entry.EntryDate.Date}");
                    return false; 
                }

                // [DATA INTEGRITY]: Explicitly set system fields to prevent user-tampering.
                entry.UserId = userId;
                entry.EntryDate = entry.EntryDate.Date; 
                entry.CreatedAt = DateTime.Now;
                entry.UpdatedAt = DateTime.Now;

                // [ALGORITHM]: Automatically calculate word count for analytics.
                entry.WordCount = CalculateWordCount(entry.Content);

                // [ENTITY FRAMEWORK]: Add the object to the change tracker and save to SQLite.
                _context.JournalEntries.Add(entry);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // [ERROR HANDLING]: Log errors to console (demonstrates defensive programming).
                Console.WriteLine($"[CRITICAL ERROR] JournalService.CreateEntryAsync: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// READ: Fetches a specific entry by date.
        /// [VIVA INFO]: Uses LINQ 'Include' (Eager Loading) to fetch related data (Tags).
        /// </summary>
        public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date, int userId)
        {
            var start = date.Date;
            var end = start.AddDays(1);
            return await _context.JournalEntries
                .Include(e => e.EntryTags) // Eager Loading: Loads Junction Table
                .ThenInclude(et => et.Tag) // Eager Loading: Loads Actual Tag Data
                .FirstOrDefaultAsync(e => e.EntryDate >= start && e.EntryDate < end && e.UserId == userId);
        }

        /// <summary>
        /// READ: Fetches an entry by its ID.
        /// </summary>
        public async Task<JournalEntry?> GetEntryByIdAsync(int id, int userId)
        {
            return await _context.JournalEntries
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        }

        /// <summary>
        /// READ: Returns all entries for a user with Pagination support.
        /// [VIVA INFO]: Pagination is important for performance with large datasets.
        /// </summary>
        public async Task<List<JournalEntry>> GetAllEntriesAsync(int userId, int page = 1, int pageSize = 10)
        {
            return await _context.JournalEntries
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EntryDate) // [UI/UX]: Show newest entries first.
                .Skip((page - 1) * pageSize) // Offset logic for pagination.
                .Take(pageSize) // Limit logic for pagination.
                .ToListAsync();
        }

        /// <summary>
        /// READ: Counts total entries for pagination calculations.
        /// </summary>
        public async Task<int> GetTotalEntriesCountAsync(int userId)
        {
            return await _context.JournalEntries.CountAsync(e => e.UserId == userId);
        }

        /// <summary>
        /// UPDATE: Modifies an existing journal entry.
        /// [VIVA INFO]: Shows manual mapping for controlled updates.
        /// </summary>
        public async Task<bool> UpdateEntryAsync(JournalEntry entry)
        {
            try
            {
                // First, find the existing record in the database.
                var existingEntry = await _context.JournalEntries
                    .FirstOrDefaultAsync(e => e.Id == entry.Id);

                if (existingEntry == null) return false;

                // [UPDATE LOGIC]: Map new values to the existing entity.
                existingEntry.Title = entry.Title;
                existingEntry.Content = entry.Content;
                existingEntry.EntryDate = entry.EntryDate.Date;
                existingEntry.PrimaryMood = entry.PrimaryMood;
                existingEntry.PrimaryMoodCategory = entry.PrimaryMoodCategory;
                existingEntry.SecondaryMood1 = entry.SecondaryMood1;
                existingEntry.SecondaryMood1Category = entry.SecondaryMood1Category;
                existingEntry.SecondaryMood2 = entry.SecondaryMood2;
                existingEntry.SecondaryMood2Category = entry.SecondaryMood2Category;
                existingEntry.Tags = entry.Tags;
                existingEntry.WordCount = CalculateWordCount(entry.Content);
                existingEntry.Pin = entry.Pin; // [SECURITY]: Update the vault PIN if changed.
                existingEntry.UpdatedAt = DateTime.Now; // Update the timestamp.

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] JournalService.UpdateEntryAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// DELETE: Removes an entry from the database.
        /// </summary>
        public async Task<bool> DeleteEntryAsync(int id)
        {
            try
            {
                var entry = await _context.JournalEntries.FindAsync(id);
                if (entry == null) return false;

                _context.JournalEntries.Remove(entry); // Mark for deletion.
                await _context.SaveChangesAsync(); // Execute deletion in SQLite.
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] JournalService.DeleteEntryAsync: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// SEARCH: Filters entries by text in Title or Content.
        /// [VIVA INFO]: Demonstrates LINQ 'Contains' which translates to SQL 'LIKE'.
        /// </summary>
        public async Task<List<JournalEntry>> SearchEntriesAsync(string searchTerm, int userId)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllEntriesAsync(userId);
            }

            return await _context.JournalEntries
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .Where(e => e.UserId == userId && 
                      (e.Title.Contains(searchTerm) || e.Content.Contains(searchTerm)))
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
        }

        /// <summary>
        /// FILTER: Filters entries within a specific date range.
        /// </summary>
        public async Task<List<JournalEntry>> FilterByDateRangeAsync(DateTime startDate, DateTime endDate, int userId)
        {
            var start = startDate.Date;
            var end = endDate.Date.AddDays(1);
            return await _context.JournalEntries
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .Where(e => e.UserId == userId && e.EntryDate >= start && e.EntryDate < end)
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
        }

        /// <summary>
        /// Helper Method: Calculates word count of a string.
        /// [VIVA INFO]: Pure C# logic used internally for data enrichment.
        /// </summary>
        private int CalculateWordCount(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return 0;

            // Split by whitespace characters and remove empty entries.
            var words = content.Split(new[] { ' ', '\n', '\r', '\t' }, 
                StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }

        /// <summary>
        /// Fetches all dates where a user has written an entry.
        /// Useful for highlighting dates in a Calendar UI.
        /// </summary>
        public async Task<List<DateTime>> GetEntryDatesAsync(int userId)
        {
            return await _context.JournalEntries
                .Where(e => e.UserId == userId)
                .Select(e => e.EntryDate.Date)
                .Distinct() // Prevent duplicate dates in the list.
                .ToListAsync();
        }

        // Check if entry exists for a specific date for specific user
        public async Task<bool> EntryExistsForDateAsync(DateTime date, int userId)
        {
            return await _context.JournalEntries
                .AnyAsync(e => e.UserId == userId && e.EntryDate.Date == date.Date);
        }
    }
}
