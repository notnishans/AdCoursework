using JournalApp.Models;

namespace JournalApp.Services
{
    /// <summary>
    /// Service for managing authentication state across the application
    /// [VIVA INFO]: This is a 'Singleton' Service (registered in MauiProgram.cs).
    /// It stays alive for the entire app session to track who is logged in.
    /// </summary>
    public class AuthenticationStateService
    {
        private User? _currentUser;

        public AuthenticationStateService()
        {
            // Initialize with a default guest user for the simplified experience
            _currentUser = new User { Id = 1, Username = "Journaler", Email = "guest@example.com" };
        }

        /// <summary>
        /// The currently logged-in user object.
        /// [VIVA INFO]: In this simplified version, we default to a single user.
        /// </summary>
        public User? CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                NotifyAuthenticationStateChanged();
            }
        }

        public bool IsAuthenticated => true; // Always authenticated in this version

        /// <summary>
        /// [EVENT DRIVEN]: An event that UI components can subscribe to.
        /// When the user logs in/out, the UI will 'react' and refresh automatically.
        /// </summary>
        public event Action? AuthenticationStateChanged;

        /// <summary>
        /// Sets the current authenticated user
        /// </summary>
        public void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        /// <summary>
        /// Notifies subscribers of authentication state changes
        /// </summary>
        private void NotifyAuthenticationStateChanged()
        {
            AuthenticationStateChanged?.Invoke();
        }

        /// <summary>
        /// Gets the current user's ID
        /// </summary>
        public int? GetCurrentUserId()
        {
            return 1; // Default to ID 1
        }

        /// <summary>
        /// Gets the current user's username
        /// </summary>
        public string GetCurrentUsername()
        {
            return _currentUser?.Username ?? "Journaler";
        }

        // Methods for app-wide PIN are removed as per requirements (Vault is now per-journal)
    }
}