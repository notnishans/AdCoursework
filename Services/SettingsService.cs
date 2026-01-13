using JournalApp.Models;
using JournalApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace JournalApp.Services
{
    /// <summary>
    /// Service for managing application-wide settings.
    /// [VIVA INFO]: Demonstrates 'Persistence' of user preferences in the database.
    /// This ensures settings like 'Theme' remain the same after closing the app.
    /// </summary>
    public class SettingsService
    {
        private readonly JournalDbContext _context;

        public event Action<string>? ThemeChanged;

        public SettingsService(JournalDbContext context)
        {
            _context = context;
        }

        private void NotifyThemeChanged(string theme)
        {
            ThemeChanged?.Invoke(theme);
        }

        /// <summary>
        /// Retrieves the global settings. If none exist, initializes a default record.
        /// [VIVA INFO]: This is an implementation of the 'Null Object Pattern' or 
        /// 'Lazy Initialization' to ensure the app never crashes due to missing settings.
        /// </summary>
        public async Task<AppSettings> GetSettingsAsync()
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();
            
            if (settings == null)
            {
                settings = new AppSettings { Theme = "Light", EntriesPerPage = 10 };
                _context.AppSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return settings;
        }

        /// <summary>
        /// Update theme setting
        /// </summary>
        public async Task<bool> UpdateThemeAsync(string theme)
        {
            try
            {
                var settings = await GetSettingsAsync();
                settings.Theme = theme;
                settings.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
                NotifyThemeChanged(theme);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateEntriesPerPageAsync(int count)
        {
            try
            {
                var settings = await GetSettingsAsync();
                settings.EntriesPerPage = count;
                settings.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
