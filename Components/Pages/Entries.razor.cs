using JournalApp.Services;
using JournalApp.Models;
using JournalApp.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace JournalApp.Components.Pages
{
    /// <summary>
    /// Code-behind for the Entries page.
    /// Demonstrates Separation of Concerns (SoC) by keeping UI logic separate from markup.
    /// Implements Dependency Injection (DI) to access services.
    /// </summary>
    public partial class Entries
    {
        [Inject] public JournalService JournalService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public AuthenticationStateService AuthState { get; set; } = default!;
        [Inject] public AuthenticationService AuthenticationService { get; set; } = default!;
        [Inject] public IDialogService DialogService { get; set; } = default!;
        [Inject] public ISnackbar Snackbar { get; set; } = default!;

        private List<JournalEntry> entries = new();
        private bool isLoading = true;
        private int currentPage = 1;
        private int pageSize = 12;
        private int totalPages = 1;
        private int totalEntries = 0;
        private string errorMessage = string.Empty; // For graceful error state handling

        // Search and filter properties
        private string searchTerm = "";
        private DateTime? filterStartDate;
        private DateTime? filterEndDate;
        private string selectedMood = "";
        private string selectedTag = "";
        private bool showFilters = false;

        protected override async Task OnInitializedAsync()
        {
            try 
            {
                // No more app-wide PIN verification needed in this version
                await LoadEntriesAsync();

                await LoadEntriesAsync();
            }
            catch (Exception ex)
            {
                // Error Handling: Graceful degradation instead of application crash
                errorMessage = "An unexpected error occurred while loading entries.";
                Console.WriteLine($"[ERROR] Entries.OnInitializedAsync: {ex.Message}");
            }
        }

        private async Task LoadEntriesAsync()
        {
            if (!AuthState.IsAuthenticated) return;
            isLoading = true;
            try
            {
                var userId = AuthState.GetCurrentUserId();
                if (userId.HasValue)
                {
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        entries = await JournalService.SearchEntriesAsync(searchTerm, userId.Value);
                        totalEntries = entries.Count;
                        totalPages = 1;
                    }
                    else if (filterStartDate.HasValue || filterEndDate.HasValue || !string.IsNullOrEmpty(selectedMood) || !string.IsNullOrEmpty(selectedTag))
                    {
                        var filtered = await JournalService.GetAllEntriesAsync(userId.Value, 1, 1000);
                        
                        if (filterStartDate.HasValue) filtered = filtered.Where(e => e.EntryDate >= filterStartDate.Value).ToList();
                        if (filterEndDate.HasValue) filtered = filtered.Where(e => e.EntryDate <= filterEndDate.Value).ToList();
                        if (!string.IsNullOrEmpty(selectedMood)) filtered = filtered.Where(e => e.PrimaryMood == selectedMood).ToList();
                        if (!string.IsNullOrEmpty(selectedTag)) filtered = filtered.Where(e => e.Tags != null && e.Tags.Contains(selectedTag)).ToList();
                        
                        entries = filtered;
                        totalEntries = entries.Count;
                        totalPages = 1;
                    }
                    else
                    {
                        totalEntries = await JournalService.GetTotalEntriesCountAsync(userId.Value);
                        totalPages = (int)Math.Ceiling((double)totalEntries / pageSize);
                        entries = await JournalService.GetAllEntriesAsync(userId.Value, currentPage, pageSize);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading entries: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task SearchEntries()
        {
            currentPage = 1;
            await LoadEntriesAsync();
        }

        private void ToggleFilters() => showFilters = !showFilters;

        private async Task HandleSearchKeyUp(KeyboardEventArgs e)
        {
            if (e.Key == "Enter") await SearchEntries();
        }

        private async Task OnPageChanged(int page)
        {
            currentPage = page;
            await LoadEntriesAsync();
        }

        private async Task ApplyAllFilters()
        {
            currentPage = 1;
            await LoadEntriesAsync();
        }

        private async Task ClearFilters()
        {
            searchTerm = "";
            filterStartDate = null;
            filterEndDate = null;
            selectedMood = "";
            selectedTag = "";
            currentPage = 1;
            await LoadEntriesAsync();
        }

        private async Task HandleEntryClick(JournalEntry entry)
        {
            if (string.IsNullOrEmpty(entry.Pin))
            {
                ViewEntry(entry.Id);
                return;
            }

            // Challenge for PIN
            var parameters = new DialogParameters { ["TargetPin"] = entry.Pin };
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
            var dialog = await DialogService.ShowAsync<JournalPinChallengeDialog>("Unlock Journal", parameters, options);
            var result = await dialog.Result;

            if (result != null && !result.Canceled)
            {
                ViewEntry(entry.Id);
            }
        }

        private void ViewEntry(int entryId) => Navigation.NavigateTo($"/create-edit-entry/{entryId}");

        private async Task ConfirmDelete(int id)
        {
            var parameters = new DialogParameters();
            parameters.Add("ContentText", "Are you sure you want to delete this entry? This action cannot be undone.");
            parameters.Add("ButtonText", "Delete");
            parameters.Add("Color", MudBlazor.Color.Error);

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
            var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Delete Entry", parameters, options);
            var result = await dialog.Result;

            if (result != null && !result.Canceled)
            {
                var success = await JournalService.DeleteEntryAsync(id);
                if (success)
                {
                    Snackbar.Add("Entry deleted successfully", Severity.Success);
                    await LoadEntriesAsync();
                }
                else
                {
                    Snackbar.Add("Failed to delete entry", Severity.Error);
                }
            }
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

        protected string GetMoodEmoji(MoodCategory category)
        {
            return category switch
            {
                MoodCategory.Positive => "😊",
                MoodCategory.Neutral => "😐",
                MoodCategory.Negative => "😔",
                _ => "😐"
            };
        }
    }
}
