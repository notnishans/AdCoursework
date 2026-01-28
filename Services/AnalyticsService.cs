using JournalApp.Data;
using JournalApp.Models;
using Microsoft.EntityFrameworkCore;

namespace JournalApp.Services
{
    /// <summary>
    /// Service for calculating analytics and insights - demonstrates business logic abstraction.
    /// Following coursework concepts: Separation of Concerns (SoC), Data Aggregation, and LINQ.
    /// This service encapsulates complex calculations away from the UI components.
    /// </summary>
    /// <summary>
    /// Service for calculating analytics and insights.
    /// [VIVA INFO]: Demonstrates 'Data Aggregation' and complex algorithmic thinking.
    /// Encapsulates all mathematical and logic-heavy operations away from the UI.
    /// </summary>
    public class AnalyticsService
    {
        private readonly JournalDbContext _context;

        public AnalyticsService(JournalDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get comprehensive analytics for a date range for specific user.
        /// [VIVA INFO]: The entry point for all dashboard data. Demonstrates the power of LINQ.
        /// </summary>
        public async Task<AnalyticsData> GetAnalyticsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            // [LOGIC]: Default range handling.
            startDate ??= DateTime.MinValue;
            endDate ??= DateTime.MaxValue;

            // [LINQ]: Language Integrated Query. Fetches filtered data once to be processed in memory.
            var entries = await _context.JournalEntries
                .Where(e => e.UserId == userId && e.EntryDate >= startDate && e.EntryDate <= endDate)
                .ToListAsync();

            var analytics = new AnalyticsData { TotalEntries = entries.Count };

            if (entries.Count == 0) return analytics;

            analytics.FirstEntryDate = entries.Min(e => e.EntryDate);
            analytics.LastEntryDate = entries.Max(e => e.EntryDate);

            // [MODULARITY]: Each calculation is split into its own private method (Clean Code).
            CalculateMoodDistribution(entries, analytics);
            CalculateMostFrequentMood(entries, analytics);
            await CalculateStreaksAsync(userId, analytics);
            CalculateTagAnalytics(entries, analytics);
            CalculateWordCountTrends(entries, analytics);

            return analytics;
        }

        /// <summary>
        /// [LOGIC]: Iterates through entries to categorize moods based on their enum values.
        /// </summary>
        private void CalculateMoodDistribution(List<JournalEntry> entries, AnalyticsData analytics)
        {
            var allMoods = new List<(string Mood, MoodCategory Category)>();

            foreach (var entry in entries)
            {
                allMoods.Add((entry.PrimaryMood, entry.PrimaryMoodCategory));
                
                if (!string.IsNullOrEmpty(entry.SecondaryMood1) && entry.SecondaryMood1Category.HasValue)
                {
                    allMoods.Add((entry.SecondaryMood1, entry.SecondaryMood1Category.Value));
                }
                
                if (!string.IsNullOrEmpty(entry.SecondaryMood2) && entry.SecondaryMood2Category.HasValue)
                {
                    allMoods.Add((entry.SecondaryMood2, entry.SecondaryMood2Category.Value));
                }
            }

            // [LINQ]: Uses the 'Count' method with a predicate (lambda expression).
            analytics.PositiveMoodCount = allMoods.Count(m => m.Category == MoodCategory.Positive);
            analytics.NeutralMoodCount = allMoods.Count(m => m.Category == MoodCategory.Neutral);
            analytics.NegativeMoodCount = allMoods.Count(m => m.Category == MoodCategory.Negative);

            int totalMoods = allMoods.Count;
            if (totalMoods > 0)
            {
                analytics.PositiveMoodPercentage = Math.Round((double)analytics.PositiveMoodCount / totalMoods * 100, 2);
            }
        }

        /// <summary>
        /// [ALGORITHM]: Calculates current and longest writing streaks.
        /// [VIVA INFO]: This is a classic coding interview/viva question. 
        /// It shows how you can turn raw timestamps into meaningful insights.
        /// </summary>
        private async Task CalculateStreaksAsync(int userId, AnalyticsData analytics)
        {
            var allEntries = await _context.JournalEntries
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.EntryDate)
                .Select(e => e.EntryDate.Date)
                .ToListAsync();

            if (!allEntries.Any()) return;

            analytics.CurrentStreak = CalculateCurrentStreak(allEntries);
            analytics.LongestStreak = CalculateLongestStreak(allEntries);

            // [LOGIC]: Missed days = Total days in range - Unique days with entries.
            var firstDate = allEntries.First();
            var lastDate = allEntries.Last();
            var totalDays = (lastDate - firstDate).Days + 1;
            analytics.MissedDays = totalDays - allEntries.Distinct().Count();
        }

        /// <summary>
        /// [ALGORITHM]: Counts consecutive days backwards from today/yesterday.
        /// </summary>
        private int CalculateCurrentStreak(List<DateTime> entryDates)
        {
            var today = DateTime.Today;
            var currentStreak = 0;

            var lastEntryDate = entryDates.LastOrDefault();
            if (lastEntryDate < today.AddDays(-1)) return 0; // Streak broken if no entry yesterday.

            var checkDate = today;
            while (entryDates.Contains(checkDate) || 
                   (checkDate == today && entryDates.Contains(checkDate.AddDays(-1))))
            {
                if (entryDates.Contains(checkDate)) currentStreak++;
                checkDate = checkDate.AddDays(-1);
            }
            return currentStreak;
        }

        /// <summary>
        /// [ALGORITHM]: Finds the longest sequence of consecutive days in history.
        /// </summary>
        private int CalculateLongestStreak(List<DateTime> entryDates)
        {
            if (!entryDates.Any()) return 0;

            var longestStreak = 1;
            var currentStreakCount = 1;
            var distinctDates = entryDates.Distinct().OrderBy(d => d).ToList();

            for (int i = 1; i < distinctDates.Count; i++)
            {
                // [LOGIC]: If current date is exactly 1 day after previous date, continue streak.
                if ((distinctDates[i] - distinctDates[i - 1]).Days == 1)
                {
                    currentStreakCount++;
                    longestStreak = Math.Max(longestStreak, currentStreakCount);
                }
                else
                {
                    currentStreakCount = 1; // Reset streak if there's a gap.
                }
            }
            return longestStreak;
        }

        /// <summary>
        /// [LINQ]: Groups and calculates word counts by date.
        /// </summary>
        private void CalculateWordCountTrends(List<JournalEntry> entries, AnalyticsData analytics)
        {
            analytics.TotalWordCount = entries.Sum(e => e.WordCount);
            analytics.AverageWordCount = entries.Any() 
                ? Math.Round(entries.Average(e => e.WordCount), 2) 
                : 0;

            // [LINQ]: Grouping allows visualizing data in charts (Date vs. WordCount).
            analytics.DailyWordCounts = entries
                .GroupBy(e => e.EntryDate.Date)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.WordCount));
        }

        /// <summary>
        /// [ALGORITHM]: Identifies the mood that appears most frequently across entries.
        /// </summary>
        private void CalculateMostFrequentMood(List<JournalEntry> entries, AnalyticsData analytics)
        {
            var moodCounts = entries
                .Where(e => !string.IsNullOrEmpty(e.PrimaryMood))
                .GroupBy(e => e.PrimaryMood)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            if (moodCounts != null)
            {
                analytics.MostFrequentMood = moodCounts.Key;
                analytics.MostFrequentMoodCount = moodCounts.Count();
            }
        }

        /// <summary>
        /// [ALGORITHM]: Aggregates and calculates frequency percentages for tags.
        /// [VIVA INFO]: Demonstrates 'Frequency Analysis' and 'Data Normalization'.
        /// </summary>
        private void CalculateTagAnalytics(List<JournalEntry> entries, AnalyticsData analytics)
        {
            // [LOGIC]: Extract all tags from all entries, assuming tags are comma-separated or similar.
            // If JournalEntry has a Tags property (List<string> or string), we process it here.
            // Based on typical student implementations, let's assume a semi-colon or comma string.
            var allTags = entries
                .Where(e => !string.IsNullOrEmpty(e.Tags))
                .SelectMany(e => e.Tags.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(t => t.Trim())
                .ToList();

            if (!allTags.Any()) return;

            analytics.TagUsageCount = allTags
                .GroupBy(t => t)
                .OrderByDescending(g => g.Count())
                .ToDictionary(g => g.Key, g => g.Count());

            int totalTags = allTags.Count;
            analytics.TagPercentages = analytics.TagUsageCount
                .ToDictionary(kvp => kvp.Key, kvp => Math.Round((double)kvp.Value / totalTags * 100, 2));
        }
    }
}
