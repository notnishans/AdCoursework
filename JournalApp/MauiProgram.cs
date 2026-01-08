using Microsoft.Extensions.Logging;
using JournalApp.Services;


namespace JournalApp;

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
			});



		builder.Services.AddMauiBlazorWebView();

		builder.Services.AddSingleton<IJournalService, JournalService>();
		builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif


		return builder.Build();
	}
}