using System;
using Maui.Controls.Sample.Issues;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var appBuilder = MauiApp.CreateBuilder();

#if IOS || ANDROID
			appBuilder.UseMauiMaps();
#endif
			appBuilder.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("FontAwesome.ttf", "FA");
					fonts.AddFont("ionicons.ttf", "Ion");
				})
				.Issue21109AddMappers()
				.Issue18720AddMappers()
				.Issue18720EditorAddMappers()
				.Issue18720DatePickerAddMappers()
				.Issue18720TimePickerAddMappers();

			return appBuilder.Build();
		}
	}
}
