========================================================================
             VIVA PREPARATION GUIDE: JOURNAL APP (MAUI BLAZOR HYBRID)
========================================================================

This file explains the project's logic, architecture, and core modules. 
Use this to defend your project during the viva.

------------------------------------------------------------------------
1. PROJECT OVERVIEW
------------------------------------------------------------------------
A secure, cross-platform journaling application built using .NET MAUI 
Blazor Hybrid. It features user authentication, daily journaling with 
mood tracking, analytics dashboards, and PDF/HTML report generation.

------------------------------------------------------------------------
2. ARCHITECTURE & TECH STACK
------------------------------------------------------------------------
* Architecture: Service-Oriented (Layered) Architecture.
    - UI Layer: Razor Components (Blazor).
    - Business Logic Layer (BLL): C# Services (JournalService, etc.).
    - Data Access Layer (DAL): Entity Framework Core (EF Core) with SQLite.
* Tech Stack:
    - .NET MAUI: Provides the native cross-platform container (Windows/Android/iOS/macOS).
    - Blazor Hybrid: Allows building the UI with Web technologies (HTML/CSS) while 
      running on the native .NET runtime for local performance.
    - MudBlazor: Material Design component library for the Blazor UI.
    - SQLite: A lightweight, serverless local database for persistent storage.

------------------------------------------------------------------------
3. DATABASE SCHEMA (Entity Framework Core)
------------------------------------------------------------------------
Defined in 'JournalDbContext.cs'.
* Users: Stores account details (Username, Email, HashedPassword, Salt).
* JournalEntries: Core table for entries (Title, Content, Moods, WordCount).
* Tags & EntryTags: Implements a Many-to-Many relationship using a 
  junction table (EntryTags) to link entries with multiple tags.

------------------------------------------------------------------------
4. CORE LOGIC & SERVICES
------------------------------------------------------------------------

A. AUTHENTICATION (AuthenticationService.cs)
* Logic: Secure user management using Hashing and Salting.
* Key Methods:
    - RegisterAsync: Generates a random Salt, hashes the password using 
      SHA-256 for security, and saves a new User object.
    - LoginAsync: Retrieves user by username, hashes the input password 
      with the stored salt, and compares it to the database hash.
    - VerifyJournalPinAsync: Uses MAUI 'SecureStorage' to store and verify 
      an app-level PIN for privacy.

B. JOURNAL MANAGEMENT (JournalService.cs)
* Logic: Encapsulates CRUD (Create, Read, Update, Delete) operations.
* Key Methods:
    - CreateEntryAsync: Validates that only one entry exists per date/user. 
      Automatically calculates WordCount before saving.
    - GetEntryByDateAsync: Uses LINQ 'Include' to load related Tags/Moods.
    - FilterByMoodAsync / FilterByTagAsync: Uses LINQ 'Where' clauses to 
      dynamically filter data from the SQLite database.

C. ANALYTICS & INSIGHTS (AnalyticsService.cs)
* Logic: Data aggregation and algorithmic processing using LINQ.
* Key Methods:
    - CalculateStreaksAsync: Calculates current and longest entry streaks 
      by iterating through sorted entry dates and checking for gaps.
    - CalculateMoodDistribution: Aggregates counts of Positive, Neutral, 
      and Negative moods to visualize emotional trends.
    - CalculateWordCountTrends: Groups entries by date to show writing 
      volume over time.

D. DATA EXPORT (ExportService.cs)
* Logic: File I/O and HTML Template rendering.
* Method: ExportToPdfAsync - It generates a professional HTML document 
  with CSS styling, populates it with journal data, and saves it locally. 
  (Note: HTML is used as it's the standard for easy PDF conversion).

------------------------------------------------------------------------
5. HOW TO DEFEND YOUR VIVA (Common Questions & Answers)
------------------------------------------------------------------------

Q: Why did you choose Blazor Hybrid instead of traditional Blazor Server/WebAssembly?
A: Hybrid allows me to use web skills for the UI while having full access to 
   native device APIs (like SecureStorage) and local performance without 
   needing a constant internet connection.

Q: How do you handle security for user passwords?
A: I never store plain passwords. I use SHA-256 hashing with a unique 'Salt' 
   for every user. This prevents 'Rainbow Table' attacks where hackers use 
   pre-computed hashes to find passwords.

Q: Explain "Dependency Injection" (DI) in your project.
A: I register my services (e.g., JournalService) in 'MauiProgram.cs'. The 
   system then "injects" these into my components automatically via the 
   constructor or '@inject' directive. This makes the code modular and testable.

Q: What is LINQ and where did you use it?
A: LINQ (Language Integrated Query) allows writing SQL-like queries directly 
   in C#. I used it extensively in 'JournalService' for searching titles 
   (e.g., '.Where(e => e.Title.Contains(searchTerm))') and in 'Analytics' 
   for calculating averages and sums.

Q: Why SQLite instead of an online database?
A: For a personal journal app, privacy and offline access are key. SQLite 
   is local, requires no server setup, and is extremely fast for mobile/desktop.

------------------------------------------------------------------------
6. DESIGN PATTERNS USED
------------------------------------------------------------------------
* Repository/Service Pattern: Separating logic (Services) from UI (Razor).
* DTO (Data Transfer Objects): Using specific models like RegisterDto for 
  security during data transport.
* Singleton/Scoped: Managing the lifecycle of services in memory using DI.

========================================================================
                      PRODUCED BY ANTIGRAVITY
========================================================================
