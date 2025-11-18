using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.Services;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Essentials.AI;

namespace Maui.Controls.Sample;

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

		builder.Services.AddPlatformChatClient();

		// Register Pages
		builder.Services.AddTransient<LandmarksPage>();
		builder.Services.AddTransient<TripPlanningPage>();

		// Register ViewModels
		builder.Services.AddTransient<LandmarksViewModel>();
		builder.Services.AddTransient<TripPlanningViewModel>();

		// Register Services
		builder.Services.AddSingleton<LandmarkDataService>(sp => LandmarkDataService.Instance);
		builder.Services.AddTransient<ItineraryService>();
		builder.Services.AddTransient<TaggingService>();

		return builder.Build();
	}
}
