using JournalApp.Services;
using JournalApp.Data;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace JournalApp.Components.Pages
{
    /// <summary>
    /// Analytics page showing comprehensive insights - demonstrates data visualization.
    /// Following coursework concepts: Dependency Injection, Data Aggregation, and Component Lifecycle.
    /// </summary>
    public partial class Analytics
    {
        [Inject] public AuthenticationService AuthenticationService { get; set; } = default!;
        [Inject] public AuthenticationStateService AuthState { get; set; } = default!;
        [Inject] public AnalyticsService AnalyticsService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public ISnackbar Snackbar { get; set; } = default!;

        private AnalyticsData? analytics;
        private bool isLoading = true;
        private string errorMessage = string.Empty; // Graceful error handling
        private DateTime? startDate;
        private DateTime? endDate;

        private DateRange _dateRange
        {
            get => new DateRange(startDate, endDate);
            set
            {
                startDate = value.Start;
                endDate = value.End;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            // Check authentication
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
            
            // Default to last 30 days
            endDate = DateTime.Today;
            startDate = DateTime.Today.AddDays(-30);
            
            try 
            {
                await LoadAnalyticsAsync();
            }
            catch (Exception ex)
            {
                // Error Handling: Catching exceptions during initialization
                errorMessage = "Failed to initialize analytics view.";
                Console.WriteLine($"[ERROR] Analytics.OnInitializedAsync: {ex.Message}");
            }
        }

        private async Task UpdateAnalytics()
        {
            errorMessage = string.Empty;
            await LoadAnalyticsAsync();
        }

        private async Task LoadAnalyticsAsync()
        {
            if (!AuthState.IsAuthenticated)
                return;

            isLoading = true;

            try
            {
                var userId = AuthState.GetCurrentUserId();
                if (userId.HasValue)
                {
                    analytics = await AnalyticsService.GetAnalyticsAsync(userId.Value, startDate, endDate);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading analytics: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }
    }
}
