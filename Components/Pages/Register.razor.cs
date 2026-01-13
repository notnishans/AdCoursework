using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using JournalApp.Models;
using JournalApp.Services;
using MudBlazor;

namespace JournalApp.Components.Pages
{
    /// <summary>
    /// Code-behind for Register page
    /// Demonstrates user registration with validation and error handling
    /// </summary>
    public partial class Register
    {
        [Inject] private AuthenticationService AuthService { get; set; } = default!;
        [Inject] private AuthenticationStateService AuthState { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

        private RegisterDto registerModel = new();
        private string errorMessage = string.Empty;
        private string successMessage = string.Empty;
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

        private async Task HandleRegister()
        {
            try
            {
                isLoading = true;
                errorMessage = string.Empty;
                successMessage = string.Empty;
                StateHasChanged();

                var result = await AuthService.RegisterAsync(registerModel);

                if (result.IsSuccess && result.User != null)
                {
                    successMessage = "Account created successfully! You can now sign in.";
                    
                    // Auto-login after successful registration
                    await Task.Delay(1500); // Show success message briefly
                    
                    var loginDto = new LoginDto
                    {
                        Username = registerModel.Username,
                        Password = registerModel.Password
                    };
                    
                    var loginResult = await AuthService.LoginAsync(loginDto);
                    
                    if (loginResult.IsSuccess && loginResult.User != null)
                    {
                        AuthState.SetCurrentUser(loginResult.User);
                        await JSRuntime.InvokeVoidAsync("localStorage.setItem", new object[] { "isAuthenticated", "true" });
                        Navigation.NavigateTo("/dashboard");
                    }
                    else
                    {
                        // Registration was successful but auto-login failed, redirect to login
                        Navigation.NavigateTo("/login");
                    }
                }
                else
                {
                    errorMessage = result.Message;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Registration failed: {ex.Message}";
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }
    }
}