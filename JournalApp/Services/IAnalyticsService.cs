using System.Collections.Generic;
using JournalApp.Models;

namespace JournalApp.Services
{
    public interface IAnalyticsService
    {
        int GetTotalEntries();
        int GetCurrentStreak();
        int GetLongestStreak();
        double GetAverageWordCount();
        Dictionary<MoodType, int> GetMoodDistribution();
        double GetMoodPercentage(MoodType mood);
    }
}