using CollaborativeWhiteboard.Core.Services;
using CollaborativeWhiteboard.ClientApp.ViewModels;
using CollaborativeWhiteboard.ClientApp.Views;
using Microsoft.Extensions.Logging;
using CollaborativeWhiteboard;


namespace CollaborativeWhiteboard.ClientApp;

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
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        // Register services, such as SignalR service, API clients, etc.
        builder.Services.AddSingleton<DrawingService>();
        builder.Services.AddSingleton<MessagingService>();
        builder.Services.AddSingleton<MainPageViewModel>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
