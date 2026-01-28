using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using JournalApp.Models;
using JournalApp.Services;
using MudBlazor;

namespace JournalApp.Components.Pages
{
    /// <summary>
    /// Code-behind for Login page
    /// Demonstrates separation of concerns between UI and logic
    /// </summary>
    public partial class Login
    {
        [Inject] private AuthenticationService AuthService { get; set; } = default!;
        [Inject] private AuthenticationStateService AuthState { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

        private LoginDto loginModel = new();
        private string errorMessage = string.Empty;
        private bool isLoading = false;

        protected override async Task OnInitializedAsync()
        {
            // Check if user is already authenticated
            if (AuthState.IsAuthenticated)
            {
                Navigation.NavigateTo("/dashboard");
            }
            await Task.CompletedTask;
        }

        private async Task HandleLogin()
        {
            try
            {
                isLoading = true;
                errorMessage = string.Empty;
                StateHasChanged();

                var result = await AuthService.LoginAsync(loginModel);

                if (result.IsSuccess && result.User != null)
                {
                    AuthState.SetCurrentUser(result.User);
                    await JSRuntime.InvokeVoidAsync("localStorage.setItem", new object[] { "isAuthenticated", "true" });
                    
                    if (loginModel.RememberMe)
                    {
                        await JSRuntime.InvokeVoidAsync("localStorage.setItem", new object[] { "username", loginModel.Username });
                    }

                    Navigation.NavigateTo("/dashboard");
                }
                else
                {
                    errorMessage = result.Message;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Auto-fill username if remembered
                try
                {
                    var rememberedUsername = await JSRuntime.InvokeAsync<string>("localStorage.getItem", new object[] { "username" });
                    if (!string.IsNullOrEmpty(rememberedUsername))
                    {
                        loginModel.Username = rememberedUsername;
                        loginModel.RememberMe = true;
                        StateHasChanged();
                    }
                }
                catch
                {
                    // Ignore localStorage errors
                }
            }
        }
    }
}