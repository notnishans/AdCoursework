using JournalApp.Services;
using JournalApp.Data;
using JournalApp.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace JournalApp.Components.Pages
{
    /// <summary>
    /// Dashboard page - demonstrates data binding, component lifecycle, and service abstraction.
    /// Following coursework concepts: Dependency Injection, Separation of Concerns, and Event Handling.
    /// </summary>
    public partial class Dashboard
    {
        [Inject] public AuthenticationStateService AuthState { get; set; } = default!;
        [Inject] public AuthenticationService AuthenticationService { get; set; } = default!;
        [Inject] public JournalService JournalService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public AnalyticsService AnalyticsService { get; set; } = default!;
        [Inject] public ISnackbar Snackbar { get; set; } = default!;
        [Inject] public IDialogService DialogService { get; set; } = default!;
        
        private AnalyticsData? analytics;
        private List<JournalEntry> recentEntries = new();
        private bool isLoading = true;
        private string errorMessage = string.Empty; // Graceful error handling state

        protected override async Task OnInitializedAsync()
        {
            try 
            {
                // Accessing AuthState - demonstrates Separation of Concerns (SoC)
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
            await LoadDashboardDataAsync();
            }
            catch (Exception ex)
            {
                errorMessage = "Failed to load dashboard data. Please try again later.";
                Console.WriteLine($"[ERROR] Dashboard.OnInitializedAsync: {ex.Message}");
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            if (!AuthState.IsAuthenticated)
                return;
                
            isLoading = true;
            
            try
            {
                var userId = AuthState.GetCurrentUserId();
                if (userId.HasValue)
                {
                    // Get analytics for current user
                    analytics = await AnalyticsService.GetAnalyticsAsync(userId.Value);

                    // Get recent entries (last 5) for current user
                    recentEntries = await JournalService.GetAllEntriesAsync(userId.Value, page: 1, pageSize: 5);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dashboard: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
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

        private void ViewEntry(int entryId)
        {
            // Navigate to entry detail page - demonstrates Navigation from PDF
            Navigation.NavigateTo($"/edit-entry/{entryId}");
        }
    }
}
