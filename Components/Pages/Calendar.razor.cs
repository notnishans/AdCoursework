using JournalApp.Services;
using JournalApp.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace JournalApp.Components.Pages
{
    /// <summary>
    /// Calendar view component - demonstrates interactive data visualization.
    /// Implementation details: Uses a custom grid to render days while maintaining abstraction.
    /// Following coursework concepts: Encapsulation, State Management, and Lifecycle Hooks.
    /// </summary>
    public partial class Calendar
    {
        [Inject] public JournalService JournalService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public AuthenticationStateService AuthState { get; set; } = default!;
        [Inject] public AuthenticationService AuthenticationService { get; set; } = default!;
        [Inject] public ISnackbar Snackbar { get; set; } = default!;

        private string errorMessage = string.Empty;

        private DateTime currentDate = DateTime.Now;
        private DateTime? selectedDate;
        private List<DateTime> calendarDays = new();
        private Dictionary<DateTime, JournalEntry> entriesByDate = new();
        private JournalEntry? selectedEntry;

        protected override async Task OnInitializedAsync()
        {
            // Lifecycle Hook: Demonstrates asynchronous initialization pattern
            try
            {
                // Dependency Injection: Utilizing centralized auth state
                if (!AuthState.IsAuthenticated)
                {
                    Navigation.NavigateTo("/login");
                    return;
                }

                var userId = AuthState.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    Navigation.NavigateTo("/login");
                    return;
                }

                // [BYPASS]: No more app-wide PIN verification needed.
                await LoadCalendar();
            }
            catch (Exception ex)
            {
                // Error Propagation & Logging: Catching exceptions at the UI boundary
                errorMessage = "Failed to load the calendar. Please refresh the page.";
                Console.WriteLine($"[CRITICAL] Calendar.OnInitializedAsync: {ex.Message}");
                Snackbar.Add("Failed to initialize calendar view.", Severity.Error);
            }
        }

        /// <summary>
        /// Loads calendar data - demonstrates service-based abstraction
        /// </summary>
        private async Task LoadCalendar()
        {
            calendarDays.Clear();
            entriesByDate.Clear();

            // Get first day of month
            var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Calculate start date (include previous month days to fill first week)
            var startDate = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek);
            
            // Calculate end date (include next month days to fill last week)
            var endDate = lastDayOfMonth.AddDays(6 - (int)lastDayOfMonth.DayOfWeek);

            // Generate calendar days
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                calendarDays.Add(date);
            }

            // Load entries for the month
            var userId = AuthState.GetCurrentUserId();
            if (userId.HasValue)
            {
                var entries = await JournalService.FilterByDateRangeAsync(startDate, endDate, userId.Value);
                
                // Use a safer way to create the dictionary in case of unexpected duplicates
                entriesByDate = new Dictionary<DateTime, JournalEntry>();
                foreach (var entry in entries)
                {
                    var dateKey = entry.EntryDate.Date;
                    if (!entriesByDate.ContainsKey(dateKey))
                    {
                        entriesByDate[dateKey] = entry;
                    }
                }
            }
        }

        protected async Task PreviousMonth()
        {
            try
            {
                currentDate = currentDate.AddMonths(-1);
                selectedDate = null;
                selectedEntry = null;
                await LoadCalendar();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading previous month: {ex.Message}";
                Snackbar.Add("Failed to load previous month.", Severity.Error);
            }
        }

        protected async Task NextMonth()
        {
            try
            {
                currentDate = currentDate.AddMonths(1);
                selectedDate = null;
                selectedEntry = null;
                await LoadCalendar();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading next month: {ex.Message}";
                Snackbar.Add("Failed to load next month.", Severity.Error);
            }
        }

        protected async Task SelectDate(DateTime date)
        {
            selectedDate = date;
            if (entriesByDate.ContainsKey(date.Date))
            {
                selectedEntry = entriesByDate[date.Date];
            }
            else
            {
                selectedEntry = null;
            }
            await Task.CompletedTask;
        }

        protected string GetDayPaperClass(DateTime day)
        {
            var isSelected = selectedDate.HasValue && day.Date == selectedDate.Value.Date;
            var otherMonth = day.Month != currentDate.Month;
            return $"day-cell {(isSelected ? "selected" : "")} {(otherMonth ? "other-month" : "")}";
        }

        protected MudBlazor.Color GetMoodColor(MoodCategory category)
        {
            return category switch
            {
                MoodCategory.Positive => MudBlazor.Color.Success,
                MoodCategory.Neutral => MudBlazor.Color.Info,
                MoodCategory.Negative => MudBlazor.Color.Error,
                _ => MudBlazor.Color.Default
            };
        }

        protected string GetMoodEmoji(string moodCategory)
        {
            return moodCategory.ToLower() switch
            {
                "positive" => "😊",
                "neutral" => "😐",
                "negative" => "😔",
                _ => "😐"
            };
        }

        protected void EditEntry(int entryId)
        {
            Navigation.NavigateTo($"/edit-entry/{entryId}");
        }

        protected void CreateEntryForDate(DateTime date)
        {
            if (date.Date < DateTime.Today)
            {
                Snackbar.Add("You cannot create reflections for past dates.", Severity.Warning);
                return;
            }
            Navigation.NavigateTo($"/create-entry?date={date:yyyy-MM-dd}");
        }
    }
}
