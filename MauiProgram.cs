using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using JournalApp.Data;
using JournalApp.Services;
using JournalApp.Models;
using MudBlazor.Services;

namespace JournalApp
{
    /// <summary>
    /// The entry point of the .NET MAUI application.
    /// [VIVA INFO]: This is where 'Dependency Injection' (DI) is configured.
    /// DI is a design pattern that allows us to develop loosely coupled code.
    /// </summary>
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Bold.ttf", "OpenSansBold");
                });

            // [UI FRAMEWORK]: Registering Blazor Hybrid and MudBlazor (UI Component library).
            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices();

            // [DEPENDENCY INJECTION]: Registering Services.
            // 1. AddScoped: A new instance is created for each connection/scope. 
            // Most of our business logic services are Scoped.
            builder.Services.AddScoped<JournalService>();
            builder.Services.AddScoped<AnalyticsService>();
            builder.Services.AddScoped<SettingsService>();
            builder.Services.AddScoped<ExportService>();
            builder.Services.AddScoped<AuthenticationService>();

            // 2. AddSingleton: ONLY ONE instance is created for the entire app lifetime.
            // Use this for services that need to maintain state across the whole app.
            builder.Services.AddSingleton<AuthenticationStateService>();

            // [DATABASE CONFIG]: Setting up the SQLite connection.
            builder.Services.AddDbContext<JournalDbContext>(options =>
            {
                string dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "JournalApp",
                    "journal.db"
                );
                
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
                options.UseSqlite($"Data Source={dbPath}");
            });

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // [DATABASE INITIALIZATION]: Ensures the DB exists and schema is ready.
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<JournalDbContext>();
                try
                {
                    // Create the database if it doesn't exist.
                    dbContext.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DB Init Error: {ex.Message}");
                }
            }

            return app;
        }
    }
}
