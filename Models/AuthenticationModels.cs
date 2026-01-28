using System.ComponentModel.DataAnnotations;

namespace JournalApp.Models
{
    /// <summary>
    /// Data Transfer Objects for authentication operations
    /// Demonstrates separation of concerns and data validation
    /// </summary>
    /// 
    public class LoginDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class AuthenticationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }
    }

    public class SetJournalPinDto
    {
        [Required(ErrorMessage = "PIN is required")]
        [StringLength(6, MinimumLength = 4, ErrorMessage = "PIN must be between 4 and 6 digits")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "PIN must contain only numbers")]
        public string Pin { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm PIN is required")]
        [Compare("Pin", ErrorMessage = "PINs do not match")]
        public string ConfirmPin { get; set; } = string.Empty;
    }

    public class VerifyJournalPinDto
    {
        [Required(ErrorMessage = "PIN is required")]
        public string Pin { get; set; } = string.Empty;
    }

    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }

    public class ChangePinDto
    {
        [Required(ErrorMessage = "Current PIN is required")]
        public string CurrentPin { get; set; } = string.Empty;

        [Required(ErrorMessage = "New PIN is required")]
        [StringLength(6, MinimumLength = 4, ErrorMessage = "PIN must be between 4 and 6 digits")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "PIN must contain only numbers")]
        public string NewPin { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm PIN is required")]
        [Compare("NewPin", ErrorMessage = "PINs do not match")]
        public string ConfirmNewPin { get; set; } = string.Empty;
    }
}