using Microsoft.Maui;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;

namespace Maui.Controls.Sample.SingleProject
{
	public static class MauiProgram
	{
		internal static bool UseBlazor = false;

		public static MauiApp CreateMauiApp()
		{
			var appBuilder = MauiApp.CreateBuilder();
			appBuilder
				.UseMauiApp<MyApp>();

			if (UseBlazor)
			{
				appBuilder.RegisterBlazorMauiWebView();
				appBuilder.Services.AddBlazorWebView();
			}

			return appBuilder.Build();
		}
	}
}
