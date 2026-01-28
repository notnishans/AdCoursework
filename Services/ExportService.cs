using JournalApp.Models;
using JournalApp.Data;
using System.Text;

namespace JournalApp.Services
{
    /// <summary>
    /// Service for exporting journal entries to HTML/PDF
    /// Simple, student-friendly approach using HTML export
    /// </summary>
    /// <summary>
    /// Service for exporting journal entries.
    /// [VIVA INFO]: Demonstrates 'File I/O' and 'Information Presentation'.
    /// This service transforms raw database records into a professional HTML report.
    /// </summary>
    public class ExportService
    {
        private readonly JournalService _journalService;

        public ExportService(JournalService journalService)
        {
            _journalService = journalService;
        }

        /// <summary>
        /// Generates an HTML report for a range of entries.
        /// [VIVA INFO]: HTML is used because it's cross-platform and can be 
        /// converted to PDF by any modern browser or specialized library.
        /// </summary>
        public async Task<string> ExportToPdfAsync(DateTime startDate, DateTime endDate, string filePath, int userId)
        {
            try
            {
                // [LOGIC]: Delegate data fetching to the JournalService (Separation of Concerns).
                var entries = await _journalService.FilterByDateRangeAsync(startDate, endDate, userId);

                if (!entries.Any()) return "No entries found for the selected date range.";

                // [FILE SYSTEM]: Ensure the output directory exists on the user's device.
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                filePath = Path.ChangeExtension(filePath, ".html");

                // [STRING BUILDING]: Constructing the HTML structure with embedded CSS for styling.
                var html = new StringBuilder();
                html.AppendLine("<!DOCTYPE html>");
                html.AppendLine("<html lang='en'>");
                html.AppendLine("<head>");
                html.AppendLine("    <meta charset='UTF-8'>");
                html.AppendLine("    <title>Journal Export</title>");
                html.AppendLine("    <style>");
                html.AppendLine("        body { font-family: sans-serif; padding: 40px; color: #333; }");
                html.AppendLine("        .entry { border-bottom: 1px solid #eee; padding: 20px 0; }");
                html.AppendLine("        .date { font-weight: bold; color: #6366f1; }");
                html.AppendLine("    </style>");
                html.AppendLine("</head>");
                html.AppendLine("<body>");
                html.AppendLine("    <h1>Personal Reflection Report</h1>");
                
                foreach (var entry in entries.OrderBy(e => e.EntryDate))
                {
                    // [DATA TRANSFORMATION]: Converting database objects into HTML elements.
                    html.AppendLine("        <div class='entry'>");
                    html.AppendLine($"            <div class='date'>{entry.EntryDate:dddd, MMMM dd, yyyy}</div>");
                    html.AppendLine($"            <h2>{EscapeHtml(entry.Title ?? "Untitled")}</h2>");
                    html.AppendLine($"            <p>{EscapeHtml(entry.Content ?? "")}</p>");
                    html.AppendLine($"            <small>Word Count: {entry.WordCount}</small>");
                    html.AppendLine("        </div>");
                }
                
                html.AppendLine("</body></html>");

                // [FILE I/O]: Writing the generated string to the local storage.
                await File.WriteAllTextAsync(filePath, html.ToString());

                return $"Successfully exported {entries.Count} entries";
            }
            catch (Exception ex)
            {
                return $"Error creating export: {ex.Message}";
            }
        }

        /// <summary>
        /// [SECURITY]: Prevents XSS (Cross-Site Scripting) by escaping special HTML characters.
        /// </summary>
        private string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}
