using Maui.Controls.Sample.Pages;
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

		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<ChatViewModel>();

		return builder.Build();
	}
}
