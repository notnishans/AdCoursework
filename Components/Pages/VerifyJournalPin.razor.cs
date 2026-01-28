using Microsoft.AspNetCore.Components;
using JournalApp.Models;
using JournalApp.Services;

namespace JournalApp.Components.Pages
{
    public partial class VerifyJournalPin
    {
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        protected override void OnInitialized()
        {
            Navigation.NavigateTo("/dashboard");
        }
    }
}
