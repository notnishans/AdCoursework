using System;
using System.Collections.Generic;
using System.Linq;
using JournalApp.Models;

namespace JournalApp.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IJournalService _journalService;

        public AnalyticsService(IJournalService journalService)
        {
            _journalService = journalService;
        }

        public int GetTotalEntries()
        {
            return _journalService.GetEntries().Count;
        }

        public int GetCurrentStreak()
        {
            var entries = _journalService.GetEntries().OrderByDescending(e => e.CreatedDate.Date).ToList();
            if (!entries.Any()) return 0;

            var today = DateTime.Today;
            // Check if user submitted today (or yesterday to keep streak alive)
            var lastEntryDate = entries.First().CreatedDate.Date;
            
            if (lastEntryDate != today && lastEntryDate != today.AddDays(-1))
            {
                return 0;
            }

            int streak = 0;
            var checkDate = lastEntryDate;

            foreach (var entry in entries)
            {
                if (entry.CreatedDate.Date == checkDate)
                {
                    streak++;
                    checkDate = checkDate.AddDays(-1);
                }
                else if (entry.CreatedDate.Date > checkDate) 
                {
                    // Should not happen if ordered and logic is correct, but ignore duplicates or future quirks
                    continue; 
                }
                else
                {
                    // Gap found
                    break;
                }
            }
            return streak;
        }

        public int GetLongestStreak()
        {
             var entries = _journalService.GetEntries().OrderBy(e => e.CreatedDate.Date).ToList();
             if (!entries.Any()) return 0;

             int maxStreak = 0;
             int currentStreak = 0;
             DateTime? lastDate = null;

             foreach (var entry in entries)
             {
                 if (lastDate == null)
                 {
                     currentStreak = 1;
                 }
                 else if (entry.CreatedDate.Date == lastDate.Value.AddDays(1))
                 {
                     currentStreak++;
                 }
                 else if (entry.CreatedDate.Date == lastDate.Value)
                 {
                     // Same day, ignore
                     continue;
                 }
                 else
                 {
                     if (currentStreak > maxStreak) maxStreak = currentStreak;
                     currentStreak = 1;
                 }
                 lastDate = entry.CreatedDate.Date;
             }
             
             if (currentStreak > maxStreak) maxStreak = currentStreak;
             return maxStreak;
        }

        public double GetAverageWordCount()
        {
            var entries = _journalService.GetEntries();
            if (!entries.Any()) return 0;

            double totalWords = entries.Sum(e => CountWords(e.Content));
            return Math.Round(totalWords / entries.Count, 1);
        }

        public Dictionary<MoodType, int> GetMoodDistribution()
        {
            var entries = _journalService.GetEntries();
            var dict = new Dictionary<MoodType, int>();
            
            foreach (var mood in Enum.GetValues<MoodType>())
            {
                dict[mood] = entries.Count(e => e.PrimaryMood == mood);
            }
            return dict;
        }

        public double GetMoodPercentage(MoodType mood)
        {
            var entries = _journalService.GetEntries();
            if (!entries.Any()) return 0;

            int count = entries.Count(e => e.PrimaryMood == mood);
            return Math.Round((double)count / entries.Count * 100, 1);
        }

        private int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return text.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }
}