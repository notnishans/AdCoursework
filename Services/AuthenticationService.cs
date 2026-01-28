using Microsoft.EntityFrameworkCore;
using JournalApp.Data;
using JournalApp.Models;
using System.Security.Cryptography;
using System.Text;

namespace JournalApp.Services
{
    /// <summary>
    /// Service for user authentication and security.
    /// [VIVA INFO]: Demonstrates defensive programming and secure data handling.
    /// Uses SHA-256 hashing to ensure user passwords are never stored in plain text.
    /// </summary>
    public class AuthenticationService
    {
        private readonly JournalDbContext _context;

        public AuthenticationService(JournalDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Registers a new user with a secure hashed password.
        /// [VIVA INFO]: This process uses 'Salting' to prevent rainbow-table attacks.
        /// </summary>
        public async Task<AuthenticationResult> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // [VALIDATION]: Check if username is already taken.
                if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
                {
                    return new AuthenticationResult { IsSuccess = false, Message = "Username already exists" };
                }

                // [VALIDATION]: Check if email is already registered.
                if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                {
                    return new AuthenticationResult { IsSuccess = false, Message = "Email already registered" };
                }

                // [SECURITY]: 1. Generate a unique random 'Salt' for this specific user.
                var salt = GenerateSalt();
                // [SECURITY]: 2. Combine the password with the salt and hash it using SHA-256.
                var passwordHash = HashPassword(registerDto.Password, salt);

                // [DATA MAPPING]: Map the DTO (Data Transfer Object) to the User Entity.
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = passwordHash, // Encrypted form saved.
                    Salt = salt, // Salt saved to allow future verification.
                    CreatedAt = DateTime.Now,
                    LastLoginAt = DateTime.Now,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return new AuthenticationResult { IsSuccess = true, Message = "Registration successful", User = user };
            }
            catch (Exception ex)
            {
                return new AuthenticationResult { IsSuccess = false, Message = $"Registration failed: {ex.Message}" };
            }
        }

        /// <summary>
        /// Authenticates user login.
        /// [VIVA INFO]: Logic: Hash(InputPassword + StoredSalt) == StoredHash.
        /// </summary>
        public async Task<AuthenticationResult> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Find user by username.
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.IsActive);

                if (user == null)
                {
                    return new AuthenticationResult { IsSuccess = false, Message = "Invalid username or password" };
                }

                // [VERIFICATION]: Re-hash the login password with the user's stored salt.
                var passwordHash = HashPassword(loginDto.Password, user.Salt);
                
                // Compare the newly computed hash with the one in the database.
                if (passwordHash != user.PasswordHash)
                {
                    return new AuthenticationResult { IsSuccess = false, Message = "Invalid username or password" };
                }

                // Update the 'last active' timestamp.
                user.LastLoginAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return new AuthenticationResult { IsSuccess = true, Message = "Login successful", User = user };
            }
            catch (Exception ex)
            {
                return new AuthenticationResult { IsSuccess = false, Message = $"Login failed: {ex.Message}" };
            }
        }

        /// <summary>
        /// [CRYPTOGRAPHY]: Generates a cryptographically strong random salt.
        /// </summary>
        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        /// <summary>
        /// [CRYPTOGRAPHY]: Computes a SHA-256 hash of (Password + Salt).
        /// SHA-256 is a one-way hashing algorithm (it cannot be reversed).
        /// </summary>
        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + salt;
                var saltedPasswordBytes = Encoding.UTF8.GetBytes(saltedPassword);
                var hashBytes = sha256.ComputeHash(saltedPasswordBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// READ: Fetches user details by username.
        /// </summary>
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        }

        /// <summary>
        /// READ: Fetches user details by their ID.
        /// </summary>
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        }

        /// <summary>
        /// UPDATE: Allows a user to update their password.
        /// [VIVA INFO]: Demonstrates the importance of verifying the 'Current Password' 
        /// before allowing a change to the database.
        /// </summary>
        public async Task<AuthenticationResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null) return new AuthenticationResult { IsSuccess = false, Message = "User not found" };

                // 1. Verify the current password first.
                var currentPasswordHash = HashPassword(currentPassword, user.Salt);
                if (currentPasswordHash != user.PasswordHash)
                {
                    return new AuthenticationResult { IsSuccess = false, Message = "Current password is incorrect" };
                }

                // 2. If verified, generate a NEW salt and NEW hash for the new password.
                var newSalt = GenerateSalt();
                var newPasswordHash = HashPassword(newPassword, newSalt);

                user.PasswordHash = newPasswordHash;
                user.Salt = newSalt;

                await _context.SaveChangesAsync();
                return new AuthenticationResult { IsSuccess = true, Message = "Password changed successfully" };
            }
            catch (Exception ex)
            {
                return new AuthenticationResult { IsSuccess = false, Message = $"Password change failed: {ex.Message}" };
            }
        }

        /// <summary>
        /// [MAUI POWER]: Uses SecureStorage to set an app-level PIN.
        /// SecureStorage uses the underlying OS (Windows Hello, Keychain, Keystore) 
        /// to store data securely.
        /// </summary>
        public async Task<AuthenticationResult> SetJournalPinAsync(int userId, SetJournalPinDto model)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new AuthenticationResult { IsSuccess = false, Message = "User not found" };

            var existingPin = await SecureStorage.GetAsync($"JournalPin_{userId}");
            if (existingPin != null) return new AuthenticationResult { IsSuccess = false, Message = "Journal PIN is already set" };

            var hashedPin = HashPassword(model.Pin, user.Salt);
            await SecureStorage.SetAsync($"JournalPin_{userId}", hashedPin);
            
            user.HasJournalPin = true;
            await _context.SaveChangesAsync();

            return new AuthenticationResult { IsSuccess = true, Message = "Journal PIN set successfully", User = user };
        }

        /// <summary>
        /// [SECURITY]: Verifies the app-level PIN.
        /// </summary>
        public async Task<AuthenticationResult> VerifyJournalPinAsync(int userId, string pin)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new AuthenticationResult { IsSuccess = false, Message = "User not found" };

            var storedHashedPin = await SecureStorage.GetAsync($"JournalPin_{userId}");
            if (string.IsNullOrEmpty(storedHashedPin)) return new AuthenticationResult { IsSuccess = false, Message = "Journal PIN is not set" };

            var hashedInputPin = HashPassword(pin, user.Salt);
            if (storedHashedPin != hashedInputPin)
            {
                return new AuthenticationResult { IsSuccess = false, Message = "Incorrect PIN" };
            }

            return new AuthenticationResult { IsSuccess = true, Message = "PIN verified successfully", User = user };
        }

        /// <summary>
        /// UPDATE: Modifies user profile information (Username/Email).
        /// [VIVA INFO]: Includes validation to ensure the new username/email isn't already taken by others.
        /// </summary>
        public async Task<AuthenticationResult> UpdateProfileAsync(int userId, UpdateProfileDto model)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                if (userId == 1)
                {
                    // Recursively ensure user 1 exists if this is the singleton guest
                    user = new User 
                    { 
                        Id = 1, Username = model.Username, Email = model.Email, 
                        PasswordHash = "bypass", Salt = "bypass", IsActive = true 
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                    return new AuthenticationResult { IsSuccess = true, Message = "Profile initialized and updated", User = user };
                }
                return new AuthenticationResult { IsSuccess = false, Message = "User not found" };
            }

            // Check if new username is taken by anyone ELSE.
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Id != userId);
            if (existingUser != null) return new AuthenticationResult { IsSuccess = false, Message = "Username is already taken" };

            // Check if new email is taken by anyone ELSE.
            existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Id != userId);
            if (existingUser != null) return new AuthenticationResult { IsSuccess = false, Message = "Email is already taken" };

            user.Username = model.Username;
            user.Email = model.Email;

            await _context.SaveChangesAsync();
            return new AuthenticationResult { IsSuccess = true, Message = "Profile updated successfully", User = user };
        }

        /// <summary>
        /// UPDATE: Changes the app-level PIN.
        /// </summary>
        public async Task<AuthenticationResult> ChangePinAsync(int userId, ChangePinDto model)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new AuthenticationResult { IsSuccess = false, Message = "User not found" };

            var storedHashedPin = await SecureStorage.GetAsync($"JournalPin_{userId}");
            if (string.IsNullOrEmpty(storedHashedPin)) return new AuthenticationResult { IsSuccess = false, Message = "Journal PIN is not set" };

            // 1. Verify CURRENT pin.
            var hashedCurrentPin = HashPassword(model.CurrentPin, user.Salt);
            if (storedHashedPin != hashedCurrentPin)
            {
                return new AuthenticationResult { IsSuccess = false, Message = "Current PIN is incorrect" };
            }

            // 2. Set NEW pin.
            var hashedNewPin = HashPassword(model.NewPin, user.Salt);
            await SecureStorage.SetAsync($"JournalPin_{userId}", hashedNewPin);

            return new AuthenticationResult { IsSuccess = true, Message = "PIN changed successfully", User = user };
        }

        /// <summary>
        /// Checks if the user has already configured a PIN.
        /// </summary>
        public async Task<bool> HasJournalPinAsync(int userId)
        {
            var pin = await SecureStorage.GetAsync($"JournalPin_{userId}");
            return !string.IsNullOrEmpty(pin);
        }
    }
}